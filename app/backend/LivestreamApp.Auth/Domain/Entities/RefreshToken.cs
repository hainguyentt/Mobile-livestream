using LivestreamApp.Shared.Domain.Primitives;

namespace LivestreamApp.Auth.Domain.Entities;

public sealed class RefreshToken : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }
    public string? ReplacedByTokenHash { get; private set; }
    public string IpAddress { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private RefreshToken() : base(Guid.Empty) { TokenHash = null!; IpAddress = null!; }

    public static RefreshToken Create(Guid userId, string tokenHash, string ipAddress, int expiryDays = 30)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = tokenHash,
            IpAddress = ipAddress,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            CreatedAt = DateTime.UtcNow
        };
    }

    // EF Core needs settable Id
    private new Guid Id { get; set; }

    public bool IsExpired() => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive() => !IsRevoked && !IsExpired();

    public void Revoke(string? replacedByTokenHash = null)
    {
        IsRevoked = true;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
