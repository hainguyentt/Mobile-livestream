using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Profiles.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Repositories;

/// <summary>EF Core implementation of IHostProfileRepository.</summary>
public class HostProfileRepository : IHostProfileRepository
{
    private readonly AppDbContext _context;

    public HostProfileRepository(AppDbContext context) => _context = context;

    public async Task<HostProfile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.HostProfiles.FindAsync([id], ct);

    public async Task<IReadOnlyList<HostProfile>> GetAllAsync(CancellationToken ct = default) =>
        await _context.HostProfiles.ToListAsync(ct);

    public async Task AddAsync(HostProfile entity, CancellationToken ct = default) =>
        await _context.HostProfiles.AddAsync(entity, ct);

    public void Update(HostProfile entity) => _context.HostProfiles.Update(entity);

    public void Remove(HostProfile entity) => _context.HostProfiles.Remove(entity);

    public async Task<HostProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await _context.HostProfiles.FirstOrDefaultAsync(h => h.Id == userId, ct);
}
