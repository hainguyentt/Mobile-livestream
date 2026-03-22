using LivestreamApp.Auth.Domain.Entities;
using LivestreamApp.Auth.Options;
using LivestreamApp.Auth.Repositories;
using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;
using LivestreamApp.Shared.Utilities;
using Microsoft.Extensions.Options;

namespace LivestreamApp.Auth.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IOtpRepository _otpRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICacheService _cache;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILineOAuthService _lineOAuth;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        IUserRepository userRepository,
        IOtpRepository otpRepository,
        IUnitOfWork unitOfWork,
        ICacheService cache,
        IEmailService emailService,
        ISmsService smsService,
        ILineOAuthService lineOAuth,
        IOptions<JwtOptions> jwtOptions)
    {
        _userRepository = userRepository;
        _otpRepository = otpRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _emailService = emailService;
        _smsService = smsService;
        _lineOAuth = lineOAuth;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<User> RegisterWithEmailAsync(string email, string password, CancellationToken ct = default)
    {
        var existing = await _userRepository.GetByEmailAsync(email, ct);
        if (existing is not null)
            throw new DomainException("An account with this email already exists.");

        var passwordHash = PasswordHasher.Hash(password);
        var user = User.Create(email, passwordHash);

        await _userRepository.AddAsync(user, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        // Send verification email
        await SendEmailOtpAsync(email, OtpPurpose.EmailVerification, ct);

        return user;
    }

    public async Task<AuthTokens> LoginWithEmailAsync(string email, string password, string ipAddress, CancellationToken ct = default)
    {
        var user = await _userRepository.GetByEmailAsync(email, ct)
            ?? throw new UnauthorizedException("Invalid email or password.");

        if (user.IsLocked())
            throw new UnauthorizedException("Account is temporarily locked. Please try again later.");

        if (!PasswordHasher.Verify(password, user.PasswordHash))
        {
            user.RecordFailedLogin();
            await _unitOfWork.SaveChangesAsync(ct);
            throw new UnauthorizedException("Invalid email or password.");
        }

        if (!user.IsEmailVerified)
            throw new UnauthorizedException("Please verify your email before logging in.");

        user.RecordSuccessfulLogin(ipAddress);
        var tokens = await IssueTokensAsync(user, ipAddress, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return tokens;
    }

    public async Task<AuthTokens> LoginWithLineAsync(string lineCode, string ipAddress, CancellationToken ct = default)
    {
        var lineToken = await _lineOAuth.ExchangeCodeForTokenAsync(lineCode, ct);
        var lineProfile = await _lineOAuth.GetUserProfileAsync(lineToken.AccessToken, ct);
        var user = await _lineOAuth.LinkOrMergeAccountAsync(lineProfile.UserId, lineProfile.Email, lineProfile.DisplayName, ct);

        var tokens = await IssueTokensAsync(user, ipAddress, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return tokens;
    }

    public async Task<AuthTokens> RefreshAccessTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await _userRepository.GetRefreshTokenAsync(tokenHash, ct)
            ?? throw new UnauthorizedException("Invalid refresh token.");

        if (!stored.IsActive())
            throw new UnauthorizedException("Refresh token has expired or been revoked.");

        var user = await _userRepository.GetByIdAsync(stored.UserId, ct)
            ?? throw new UnauthorizedException("User not found.");

        // Rotate token
        var newRefreshToken = TokenGenerator.GenerateRefreshToken();
        var newTokenHash = HashToken(newRefreshToken);
        stored.Revoke(newTokenHash);

        var newStored = RefreshToken.Create(user.Id, newTokenHash, ipAddress);
        await _userRepository.AddRefreshTokenAsync(newStored, ct);

        var accessToken = TokenGenerator.GenerateAccessToken(
            user.Id, user.Email.Value, user.Role.ToString(),
            _jwtOptions.SecretKey, _jwtOptions.AccessTokenExpiryMinutes);

        await _unitOfWork.SaveChangesAsync(ct);

        return new AuthTokens(accessToken, newRefreshToken);
    }

    public async Task SendEmailOtpAsync(string email, OtpPurpose purpose, CancellationToken ct = default)
    {
        var code = TokenGenerator.GenerateOtpCode();
        var codeHash = HashToken(code);
        var otp = OtpCode.Create(email, codeHash, purpose);

        await _otpRepository.InvalidatePreviousAsync(email, purpose, ct);
        await _otpRepository.AddAsync(otp, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _emailService.SendOtpAsync(email, code, purpose, ct);
    }

    public async Task<bool> VerifyEmailOtpAsync(string email, string code, OtpPurpose purpose, CancellationToken ct = default)
    {
        var codeHash = HashToken(code);
        var otp = await _otpRepository.GetActiveAsync(email, purpose, ct);

        if (otp is null || !otp.IsValid())
            return false;

        if (otp.CodeHash != codeHash)
        {
            otp.RecordAttempt();
            await _unitOfWork.SaveChangesAsync(ct);
            return false;
        }

        otp.MarkUsed();

        if (purpose == OtpPurpose.EmailVerification)
        {
            var user = await _userRepository.GetByEmailAsync(email, ct);
            user?.VerifyEmail();
        }

        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    public async Task SendPhoneOtpAsync(string phoneNumber, CancellationToken ct = default)
    {
        var code = TokenGenerator.GenerateOtpCode();
        var codeHash = HashToken(code);
        var otp = OtpCode.Create(phoneNumber, codeHash, OtpPurpose.PhoneVerification);

        await _otpRepository.InvalidatePreviousAsync(phoneNumber, OtpPurpose.PhoneVerification, ct);
        await _otpRepository.AddAsync(otp, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        await _smsService.SendOtpAsync(phoneNumber, code, ct);
    }

    public async Task<bool> VerifyPhoneOtpAsync(Guid userId, string phoneNumber, string code, CancellationToken ct = default)
    {
        var codeHash = HashToken(code);
        var otp = await _otpRepository.GetActiveAsync(phoneNumber, OtpPurpose.PhoneVerification, ct);

        if (otp is null || !otp.IsValid())
            return false;

        if (otp.CodeHash != codeHash)
        {
            otp.RecordAttempt();
            await _unitOfWork.SaveChangesAsync(ct);
            return false;
        }

        otp.MarkUsed();

        var user = await _userRepository.GetByIdAsync(userId, ct);
        user?.VerifyPhone(phoneNumber);

        await _unitOfWork.SaveChangesAsync(ct);
        return true;
    }

    public async Task ResetPasswordAsync(string email, string newPassword, string otpCode, CancellationToken ct = default)
    {
        var isValid = await VerifyEmailOtpAsync(email, otpCode, OtpPurpose.PasswordReset, ct);
        if (!isValid)
            throw new DomainException("Invalid or expired OTP code.");

        var user = await _userRepository.GetByEmailAsync(email, ct)
            ?? throw new NotFoundException(nameof(User), email);

        user.UpdatePassword(PasswordHasher.Hash(newPassword));

        // BR-AUTH-05-3: revoke all refresh tokens after password reset
        await _userRepository.RevokeAllRefreshTokensAsync(user.Id, ct);

        await _unitOfWork.SaveChangesAsync(ct);
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default)
    {
        var tokenHash = HashToken(refreshToken);
        var stored = await _userRepository.GetRefreshTokenAsync(tokenHash, ct);
        if (stored is null) return;

        stored.Revoke();
        await _unitOfWork.SaveChangesAsync(ct);
    }

    // --- Private helpers ---

    private async Task<AuthTokens> IssueTokensAsync(User user, string ipAddress, CancellationToken ct)
    {
        var accessToken = TokenGenerator.GenerateAccessToken(
            user.Id, user.Email.Value, user.Role.ToString(),
            _jwtOptions.SecretKey, _jwtOptions.AccessTokenExpiryMinutes);

        var refreshToken = TokenGenerator.GenerateRefreshToken();
        var tokenHash = HashToken(refreshToken);
        var stored = RefreshToken.Create(user.Id, tokenHash, ipAddress, _jwtOptions.RefreshTokenExpiryDays);

        await _userRepository.AddRefreshTokenAsync(stored, ct);

        return new AuthTokens(accessToken, refreshToken);
    }

    private static string HashToken(string token)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        return Convert.ToBase64String(sha.ComputeHash(bytes));
    }
}
