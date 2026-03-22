namespace LivestreamApp.Shared.Interfaces;

/// <summary>Marker interface for domain events.</summary>
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredAt { get; }
}
