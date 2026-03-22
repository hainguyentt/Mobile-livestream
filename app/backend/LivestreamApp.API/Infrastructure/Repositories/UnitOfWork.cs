using LivestreamApp.Shared.Interfaces;

namespace LivestreamApp.API.Infrastructure.Repositories;

/// <summary>EF Core implementation of IUnitOfWork.</summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context) => _context = context;

    /// <inheritdoc/>
    public Task<int> SaveChangesAsync(CancellationToken ct = default) =>
        _context.SaveChangesAsync(ct);
}
