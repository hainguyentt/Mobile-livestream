using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.Shared.Domain.Primitives;

/// <summary>Base class for all domain entities with strongly-typed ID.</summary>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id) => Id = id;

    public TId Id { get; private set; }

    private readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) =>
        _domainEvents.Add(domainEvent);

    public void ClearDomainEvents() => _domainEvents.Clear();

    public bool Equals(Entity<TId>? other) =>
        other is not null && Id.Equals(other.Id);

    public override bool Equals(object? obj) =>
        obj is Entity<TId> entity && Equals(entity);

    public override int GetHashCode() => Id.GetHashCode();
}
