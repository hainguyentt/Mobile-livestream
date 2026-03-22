using LivestreamApp.Auth.Domain.Entities;

namespace LivestreamApp.Auth.Services;

public record LineTokenResponse(string AccessToken, string IdToken, string RefreshToken);
public record LineUserProfile(string UserId, string DisplayName, string? Email, string? PictureUrl);

public interface ILineOAuthService
{
    string GetAuthorizationUrl(string state);
    Task<LineTokenResponse> ExchangeCodeForTokenAsync(string code, CancellationToken ct = default);
    Task<LineUserProfile> GetUserProfileAsync(string accessToken, CancellationToken ct = default);
    Task<User> LinkOrMergeAccountAsync(string lineUserId, string? lineEmail, string displayName, CancellationToken ct = default);
}
