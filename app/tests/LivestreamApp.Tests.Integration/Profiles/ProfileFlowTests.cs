using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using LivestreamApp.Tests.Integration.Infrastructure;
using LivestreamApp.Shared.Domain.Enums;
using System.Net;
using System.Net.Http.Json;

namespace LivestreamApp.Tests.Integration.Profiles;

public class ProfileFlowTests : IntegrationTestBase
{
    public ProfileFlowTests(IntegrationTestFactory factory) : base(factory) { }

    [DockerAvailableFact]
    public async Task CreateProfile_AfterLogin_Returns200()
    {
        await RegisterVerifyLoginAsync("profile@example.com", "Password123!");

        var response = await PostAsync("/api/v1/profiles/me", new
        {
            displayName = "TestUser",
            dateOfBirth = "1990-01-01"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [DockerAvailableFact]
    public async Task CreateProfile_DuplicateDisplayName_Returns400()
    {
        await RegisterVerifyLoginAsync("user1@example.com", "Password123!");
        await PostAsync("/api/v1/profiles/me", new
        {
            displayName = "UniqueUser",
            dateOfBirth = "1990-01-01"
        });

        // Second user tries same display name
        await RegisterVerifyLoginAsync("user2@example.com", "Password123!");
        var response = await PostAsync("/api/v1/profiles/me", new
        {
            displayName = "UniqueUser",
            dateOfBirth = "1990-01-01"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [DockerAvailableFact]
    public async Task GetProfile_AfterCreate_ReturnsProfile()
    {
        await RegisterVerifyLoginAsync("getprofile@example.com", "Password123!");
        await PostAsync("/api/v1/profiles/me", new
        {
            displayName = "GetProfileUser",
            dateOfBirth = "1990-06-15"
        });

        var response = await Client.GetAsync("/api/v1/profiles/me");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("GetProfileUser");
    }

    [DockerAvailableFact]
    public async Task UpdateProfile_ValidData_Returns200()
    {
        await RegisterVerifyLoginAsync("update@example.com", "Password123!");
        await PostAsync("/api/v1/profiles/me", new
        {
            displayName = "UpdateUser",
            dateOfBirth = "1990-01-01"
        });

        var response = await Client.PutAsJsonAsync("/api/v1/profiles/me", new
        {
            bio = "Hello world",
            interests = new[] { "music", "gaming" },
            preferredLanguage = "ja"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Hello world");
    }

    [DockerAvailableFact]
    public async Task PresignPhoto_ValidRequest_ReturnsUploadUrl()
    {
        await RegisterVerifyLoginAsync("photo@example.com", "Password123!");

        var response = await PostAsync("/api/v1/profiles/photos/presign", new
        {
            displayIndex = 0,
            contentType = "image/jpeg",
            fileSizeBytes = 1_000_000
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("uploadUrl");
        body.Should().Contain("photoId");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task RegisterVerifyLoginAsync(string email, string password)
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

        await PostAsync("/api/v1/auth/login", new { email, password });
    }
}
