using FluentAssertions;
using LivestreamApp.API.Infrastructure;
using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Tests.Integration.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace LivestreamApp.Tests.Integration.Auth;

public class AuthFlowTests : IntegrationTestBase
{
    public AuthFlowTests(IntegrationTestFactory factory) : base(factory) { }

    // ── Register ──────────────────────────────────────────────────────────────

    [DockerAvailableFact]
    public async Task Register_ValidEmail_Returns200AndSendsOtp()
    {
        var response = await PostAsync("/api/v1/auth/register", new
        {
            email = "user@example.com",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // OTP should be created in DB
        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.OtpCodes.Should().HaveCount(1);
    }

    [DockerAvailableFact]
    public async Task Register_DuplicateEmail_Returns400()
    {
        await PostAsync("/api/v1/auth/register", new
        {
            email = "dup@example.com",
            password = "Password123!"
        });

        var response = await PostAsync("/api/v1/auth/register", new
        {
            email = "dup@example.com",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("already exists");
    }

    [DockerAvailableFact]
    public async Task Register_WeakPassword_Returns400()
    {
        var response = await PostAsync("/api/v1/auth/register", new
        {
            email = "user@example.com",
            password = "weak"
        });

        // FluentValidation returns 400 BadRequest for validation failures
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    // ── Register → Verify Email → Login ──────────────────────────────────────

    [DockerAvailableFact]
    public async Task FullAuthFlow_RegisterVerifyLogin_Success()
    {
        const string email = "flow@example.com";
        const string password = "Password123!";

        // 1. Register
        var registerResponse = await PostAsync("/api/v1/auth/register", new { email, password });
        registerResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Get OTP from TestEmailService (captures raw code before hashing)
        var otpCode = Factory.EmailService.GetLatestCode(email, OtpPurpose.EmailVerification);
        otpCode.Should().NotBeNullOrEmpty();

        // 3. Verify email OTP
        var verifyResponse = await PostAsync("/api/v1/auth/otp/email/verify", new
        {
            target = email,
            code = otpCode,
            purpose = "EmailVerification"
        });
        verifyResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Login
        var loginResponse = await PostAsync("/api/v1/auth/login", new { email, password });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Cookies should be set
        loginResponse.Headers.Should().ContainKey("Set-Cookie");
        var cookies = loginResponse.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.Contains("access_token"));
        cookies.Should().Contain(c => c.Contains("refresh_token"));
    }

    // ── Login ─────────────────────────────────────────────────────────────────

    [DockerAvailableFact]
    public async Task Login_UnverifiedEmail_Returns401()
    {
        await PostAsync("/api/v1/auth/register", new
        {
            email = "unverified@example.com",
            password = "Password123!"
        });

        var response = await PostAsync("/api/v1/auth/login", new
        {
            email = "unverified@example.com",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [DockerAvailableFact]
    public async Task Login_WrongPassword_Returns401AndIncrementsFailedCount()
    {
        await RegisterAndVerifyAsync("locktest@example.com", "Password123!");

        var response = await PostAsync("/api/v1/auth/login", new
        {
            email = "locktest@example.com",
            password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        // Load all users into memory first to avoid LINQ translation issue with ValueObject
        var user = db.Users.AsEnumerable().First(u => u.Email.Value == "locktest@example.com");
        user.FailedLoginCount.Should().Be(1);
    }

    [DockerAvailableFact]
    public async Task Login_NonExistentUser_Returns401()
    {
        var response = await PostAsync("/api/v1/auth/login", new
        {
            email = "nobody@example.com",
            password = "Password123!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    // ── Token Refresh ─────────────────────────────────────────────────────────

    [DockerAvailableFact]
    public async Task RefreshToken_ValidToken_RotatesTokens()
    {
        await RegisterAndVerifyAsync("refresh@example.com", "Password123!");
        await PostAsync("/api/v1/auth/login", new
        {
            email = "refresh@example.com",
            password = "Password123!"
        });

        // Refresh token is in httpOnly cookie — client sends it automatically
        var refreshResponse = await PostAsync("/api/v1/auth/refresh", new { });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var cookies = refreshResponse.Headers.GetValues("Set-Cookie").ToList();
        cookies.Should().Contain(c => c.Contains("access_token"));
    }

    // ── Password Reset ────────────────────────────────────────────────────────

    [DockerAvailableFact]
    public async Task SendPasswordResetOtp_ExistingEmail_Returns200()
    {
        await RegisterAndVerifyAsync("reset@example.com", "Password123!");

        var response = await PostAsync("/api/v1/auth/otp/email/send", new
        {
            target = "reset@example.com",
            purpose = "PasswordReset"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [DockerAvailableFact]
    public async Task SendPasswordResetOtp_NonExistentEmail_Returns200AlsoToPreventLeak()
    {
        // BR-AUTH-05-4: always return success even if email doesn't exist
        var response = await PostAsync("/api/v1/auth/otp/email/send", new
        {
            target = "ghost@example.com",
            purpose = "PasswordReset"
        });

        // Should not leak whether email exists
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    // ── Protected Endpoint ────────────────────────────────────────────────────

    [DockerAvailableFact]
    public async Task GetProfile_WithoutAuth_Returns401()
    {
        var response = await Client.GetAsync("/api/v1/profiles/me");
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [DockerAvailableFact]
    public async Task GetProfile_AfterLogin_Returns404WhenNoProfile()
    {
        await RegisterAndVerifyAsync("noprofile@example.com", "Password123!");
        await PostAsync("/api/v1/auth/login", new
        {
            email = "noprofile@example.com",
            password = "Password123!"
        });

        var response = await Client.GetAsync("/api/v1/profiles/me");

        // Profile doesn't exist yet — 404
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Registers a user and verifies their email via TestEmailService OTP capture.</summary>
    protected async Task RegisterAndVerifyAsync(string email, string password)
    {
        await PostAsync("/api/v1/auth/register", new { email, password });

        var code = Factory.EmailService.GetLatestCode(email, OtpPurpose.EmailVerification);
        code.Should().NotBeNullOrEmpty("OTP should have been captured by TestEmailService");

        await PostAsync("/api/v1/auth/otp/email/verify", new
        {
            target = email,
            code,
            purpose = "EmailVerification"
        });
    }
}
