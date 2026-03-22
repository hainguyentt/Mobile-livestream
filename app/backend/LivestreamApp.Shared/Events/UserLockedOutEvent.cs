using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Shared.Events;

public sealed record UserLockedOutEvent(Guid UserId, string Email, DateTime LockoutUntil) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
