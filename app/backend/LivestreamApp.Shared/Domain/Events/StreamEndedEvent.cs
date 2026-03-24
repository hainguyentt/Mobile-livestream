using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Shared.Domain.Events;

public sealed record StreamEndedEvent(
    Guid RoomId,
    Guid HostId,
    DateTime EndedAt,
    int TotalViewers,
    int PeakViewers) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
