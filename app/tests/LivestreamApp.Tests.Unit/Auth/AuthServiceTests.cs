using FluentAssertions;
using LivestreamApp.Auth.Domain.Entities;
using LivestreamApp.Auth.Options;
using LivestreamApp.Auth.Repositories;
using LivestreamApp.Auth.Services;
using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;
using Microsoft.Extensions.Options;
using Moq;

namespace LivestreamApp.Tests.Unit.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _userRepo = new();
    private readonly Mock<IOtpRepository> _otpRepo = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly Mock<ICacheService> _cache = new();
    private readonly Mock<IEmailService> _emailService = new();
    private readonly Mock<ISmsService> _smsService = new();
    private readonly Mock<ILineOAuthService> _lineOAuth = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var jwtOptions = Options.Create(new JwtOptions
        {
            SecretKey = "test-secret-key-that-is-long-enough-32chars",
            AccessTokenExpiryMinutes = 15,
            RefreshTokenExpiryDays = 30
        });

        _sut = new AuthService(
            _userRepo.Object, _otpRepo.Object, _uow.Object,
            _cache.Object, _emailService.Object, _smsService.Object,
            _lineOAuth.Object, jwtOptions);
    }

    [Fact]
    public async Task RegisterWithEmail_Success_ReturnsUser()
    {
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync((User?)null);
        _otpRepo.Setup(r => r.InvalidatePreviousAsync(It.IsAny<string>(), It.IsAny<OtpPurpose>(), default)).Returns(Task.CompletedTask);
        _otpRepo.Setup(r => r.AddAsync(It.IsAny<OtpCode>(), default)).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);
        _emailService.Setup(e => e.SendOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OtpPurpose>(), default)).Returns(Task.CompletedTask);

        var result = await _sut.RegisterWithEmailAsync("test@example.com", "Password123!");

        result.Should().NotBeNull();
        result.Email.Value.Should().Be("test@example.com");
        result.Status.Should().Be(UserStatus.PendingVerification);
        _userRepo.Verify(r => r.AddAsync(It.IsAny<User>(), default), Times.Once);
    }

    [Fact]
    public async Task RegisterWithEmail_DuplicateEmail_ThrowsDomainException()
    {
        var existing = User.Create("test@example.com", "hash");
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(existing);

        var act = () => _sut.RegisterWithEmailAsync("test@example.com", "Password123!");

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already exists*");
    }

    [Fact]
    public async Task LoginWithEmail_Success_ReturnsTokens()
    {
        var user = User.Create("test@example.com", Shared.Utilities.PasswordHasher.Hash("Password123!"));
        user.VerifyEmail();
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _userRepo.Setup(r => r.AddRefreshTokenAsync(It.IsAny<RefreshToken>(), default)).Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _sut.LoginWithEmailAsync("test@example.com", "Password123!", "127.0.0.1");

        result.Should().NotBeNull();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task LoginWithEmail_InvalidPassword_ThrowsUnauthorized()
    {
        var user = User.Create("test@example.com", Shared.Utilities.PasswordHasher.Hash("CorrectPassword!"));
        user.VerifyEmail();
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var act = () => _sut.LoginWithEmailAsync("test@example.com", "WrongPassword!", "127.0.0.1");

        await act.Should().ThrowAsync<UnauthorizedException>();
        user.FailedLoginCount.Should().Be(1);
    }

    [Fact]
    public async Task LoginWithEmail_5thFailure_RequiresCaptcha()
    {
        var user = User.Create("test@example.com", Shared.Utilities.PasswordHasher.Hash("CorrectPassword!"));
        user.VerifyEmail();
        // Simulate 4 previous failures
        for (int i = 0; i < 4; i++) user.RecordFailedLogin();

        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _sut.LoginWithEmailAsync("test@example.com", "WrongPassword!", "127.0.0.1"));

        user.RequiresCaptcha.Should().BeTrue();
    }

    [Fact]
    public async Task LoginWithEmail_10thFailure_LocksAccount()
    {
        var user = User.Create("test@example.com", Shared.Utilities.PasswordHasher.Hash("CorrectPassword!"));
        user.VerifyEmail();
        for (int i = 0; i < 9; i++) user.RecordFailedLogin();

        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        await Assert.ThrowsAsync<UnauthorizedException>(
            () => _sut.LoginWithEmailAsync("test@example.com", "WrongPassword!", "127.0.0.1"));

        user.IsLocked().Should().BeTrue();
    }

    [Fact]
    public async Task VerifyEmailOtp_Success_ReturnsTrue()
    {
        var code = "123456";
        var codeHash = HashToken(code);
        var otp = OtpCode.Create("test@example.com", codeHash, OtpPurpose.EmailVerification);
        var user = User.Create("test@example.com", "hash");

        _otpRepo.Setup(r => r.GetActiveAsync("test@example.com", OtpPurpose.EmailVerification, default)).ReturnsAsync(otp);
        _userRepo.Setup(r => r.GetByEmailAsync("test@example.com", default)).ReturnsAsync(user);
        _uow.Setup(u => u.SaveChangesAsync(default)).ReturnsAsync(1);

        var result = await _sut.VerifyEmailOtpAsync("test@example.com", code, OtpPurpose.EmailVerification);

        result.Should().BeTrue();
        user.IsEmailVerified.Should().BeTrue();
    }

    [Fact]
    public async Task VerifyEmailOtp_Expired_ReturnsFalse()
    {
        _otpRepo.Setup(r => r.GetActiveAsync("test@example.com", OtpPurpose.EmailVerification, default))
            .ReturnsAsync((OtpCode?)null);

        var result = await _sut.VerifyEmailOtpAsync("test@example.com", "123456", OtpPurpose.EmailVerification);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task RefreshAccessToken_RevokedToken_ThrowsUnauthorized()
    {
        _userRepo.Setup(r => r.GetRefreshTokenAsync(It.IsAny<string>(), default))
            .ReturnsAsync((RefreshToken?)null);

        var act = () => _sut.RefreshAccessTokenAsync("invalid-token", "127.0.0.1");

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    private static string HashToken(string token)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }
}
