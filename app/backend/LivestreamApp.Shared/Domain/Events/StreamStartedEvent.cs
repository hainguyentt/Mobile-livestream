using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Shared.Domain.Events;

public sealed record StreamStartedEvent(
    Guid RoomId,
    Guid HostId,
    string Title,
    RoomCategory Category,
    DateTime StartedAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
