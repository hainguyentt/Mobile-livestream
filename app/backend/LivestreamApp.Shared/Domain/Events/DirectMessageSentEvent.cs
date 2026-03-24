using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Shared.Domain.Events;

public sealed record DirectMessageSentEvent(
    Guid ConversationId,
    Guid MessageId,
    Guid SenderId,
    Guid RecipientId) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
