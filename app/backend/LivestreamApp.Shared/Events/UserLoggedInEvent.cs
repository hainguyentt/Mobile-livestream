using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Shared.Events;

public sealed record UserLoggedInEvent(Guid UserId, string IpAddress, DateTime LoggedInAt) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredAt { get; } = DateTime.UtcNow;
}
