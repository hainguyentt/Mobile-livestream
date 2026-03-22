using LivestreamApp.Shared.Domain.Primitives;

namespace LivestreamApp.Shared.Interfaces;

public interface IRepository<TEntity, TId>
    where TEntity : Entity<TId>
    where TId : notnull
{
    Task<TEntity?> GetByIdAsync(TId id, CancellationToken ct = default);
    Task<IReadOnlyList<TEntity>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(TEntity entity, CancellationToken ct = default);
    void Update(TEntity entity);
    void Remove(TEntity entity);
}
