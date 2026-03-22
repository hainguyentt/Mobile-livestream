using LivestreamApp.Auth.Domain.Entities;
using LivestreamApp.Shared.Domain.Enums;

namespace LivestreamApp.Auth.Services;

public record AuthTokens(string AccessToken, string RefreshToken);

public interface IAuthService
{
    Task<User> RegisterWithEmailAsync(string email, string password, CancellationToken ct = default);
    Task<AuthTokens> LoginWithEmailAsync(string email, string password, string ipAddress, CancellationToken ct = default);
    Task<AuthTokens> LoginWithLineAsync(string lineCode, string ipAddress, CancellationToken ct = default);
    Task<AuthTokens> RefreshAccessTokenAsync(string refreshToken, string ipAddress, CancellationToken ct = default);
    Task SendEmailOtpAsync(string email, OtpPurpose purpose, CancellationToken ct = default);
    Task<bool> VerifyEmailOtpAsync(string email, string code, OtpPurpose purpose, CancellationToken ct = default);
    Task SendPhoneOtpAsync(string phoneNumber, CancellationToken ct = default);
    Task<bool> VerifyPhoneOtpAsync(Guid userId, string phoneNumber, string code, CancellationToken ct = default);
    Task ResetPasswordAsync(string email, string newPassword, string otpCode, CancellationToken ct = default);
    Task RevokeRefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}
