using LivestreamApp.Auth.Domain.Entities;
using LivestreamApp.Auth.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Repositories;

/// <summary>EF Core implementation of IUserRepository.</summary>
public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context) => _context = context;

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.Users.FindAsync([id], ct);

    public async Task<IReadOnlyList<User>> GetAllAsync(CancellationToken ct = default) =>
        await _context.Users.ToListAsync(ct);

    public async Task AddAsync(User entity, CancellationToken ct = default) =>
        await _context.Users.AddAsync(entity, ct);

    public void Update(User entity) => _context.Users.Update(entity);

    public void Remove(User entity) => _context.Users.Remove(entity);

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        // Email.Create normalizes to lowercase — ValueConverter stores the normalized value.
        // EF Core translates the ValueConverter on both sides of the comparison automatically.
        var emailVo = LivestreamApp.Shared.Domain.ValueObjects.Email.Create(email);
        return await _context.Users
            .Include(u => u.RefreshTokens)
            .Include(u => u.ExternalLogins)
            .FirstOrDefaultAsync(u => u.Email == emailVo, ct);
    }

    public async Task<User?> GetByExternalLoginAsync(string provider, string providerUserId, CancellationToken ct = default) =>
        await _context.Users
            .Include(u => u.ExternalLogins)
            .FirstOrDefaultAsync(u => u.ExternalLogins.Any(e => e.Provider == provider && e.ProviderUserId == providerUserId), ct);

    public async Task<RefreshToken?> GetRefreshTokenAsync(string tokenHash, CancellationToken ct = default) =>
        await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash, ct);

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default) =>
        await _context.RefreshTokens.AddAsync(token, ct);

    public async Task AddExternalLoginAsync(ExternalLogin login, CancellationToken ct = default) =>
        await _context.ExternalLogins.AddAsync(login, ct);

    public async Task RevokeAllRefreshTokensAsync(Guid userId, CancellationToken ct = default)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && !t.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
            token.Revoke();
    }
}
