using LivestreamApp.Shared.Domain.Primitives;

namespace LivestreamApp.Auth.Domain.Entities;

public sealed class ExternalLogin : Entity<Guid>
{
    public Guid UserId { get; private set; }
    public string Provider { get; private set; }   // "LINE"
    public string ProviderUserId { get; private set; }
    public string? ProviderEmail { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ExternalLogin() : base(Guid.Empty) { Provider = null!; ProviderUserId = null!; }

    public static ExternalLogin Create(Guid userId, string provider, string providerUserId, string? providerEmail = null)
    {
        return new ExternalLogin
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Provider = provider,
            ProviderUserId = providerUserId,
            ProviderEmail = providerEmail,
            CreatedAt = DateTime.UtcNow
        };
    }

    private new Guid Id { get; set; }
}
