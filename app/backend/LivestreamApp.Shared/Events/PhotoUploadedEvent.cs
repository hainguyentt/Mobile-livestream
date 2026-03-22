using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Shared.Events;

public sealed record PhotoUploadedEvent(Guid UserId, Guid PhotoId, int DisplayIndex, string S3Key) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
