namespace LivestreamApp.Shared.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
