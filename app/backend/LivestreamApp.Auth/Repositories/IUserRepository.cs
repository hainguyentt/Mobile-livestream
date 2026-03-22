using LivestreamApp.Auth.Domain.Entities;
using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Auth.Repositories;

public interface IUserRepository : IRepository<User, Guid>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetByExternalLoginAsync(string provider, string providerUserId, CancellationToken ct = default);
    Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken ct = default);
    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
    Task AddExternalLoginAsync(ExternalLogin login, CancellationToken ct = default);

    /// <summary>Revokes all active refresh tokens for a user (e.g., after password reset or account ban).</summary>
    Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken ct = default);
}
