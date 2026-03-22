using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Shared.Events;

public sealed record UserEmailVerifiedEvent(Guid UserId, string Email) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
