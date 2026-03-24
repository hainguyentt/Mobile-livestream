using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Shared.Domain.Events;

public sealed record CallAcceptedEvent(
    Guid RequestId,
    Guid SessionId,
    Guid HostId,
    Guid ViewerId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
