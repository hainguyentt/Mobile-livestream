using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Shared.Domain.Events;

public sealed record CallEndedEvent(
    Guid SessionId,
    string EndedBy,
    int TotalCoins,
    int DurationSeconds) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
