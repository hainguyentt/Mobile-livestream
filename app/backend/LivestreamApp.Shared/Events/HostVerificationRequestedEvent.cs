using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Shared.Events;

public sealed record HostVerificationRequestedEvent(Guid UserId, DateTime RequestedAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
