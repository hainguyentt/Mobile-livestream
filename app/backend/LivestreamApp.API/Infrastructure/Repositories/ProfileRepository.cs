using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Profiles.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Repositories;

/// <summary>EF Core implementation of IProfileRepository.</summary>
public class ProfileRepository : IProfileRepository
{
    private readonly AppDbContext _context;

    public ProfileRepository(AppDbContext context) => _context = context;

    public async Task<UserProfile?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.UserProfiles.FindAsync([id], ct);

    public async Task<IReadOnlyList<UserProfile>> GetAllAsync(CancellationToken ct = default) =>
        await _context.UserProfiles.ToListAsync(ct);

    public async Task AddAsync(UserProfile entity, CancellationToken ct = default) =>
        await _context.UserProfiles.AddAsync(entity, ct);

    public void Update(UserProfile entity) => _context.UserProfiles.Update(entity);

    public void Remove(UserProfile entity) => _context.UserProfiles.Remove(entity);

    public async Task<UserProfile?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await _context.UserProfiles.FirstOrDefaultAsync(p => p.Id == userId, ct);

    public async Task<bool> IsDisplayNameTakenAsync(string displayName, CancellationToken ct = default) =>
        await _context.UserProfiles
            .AnyAsync(p => p.DisplayName.ToLower() == displayName.ToLower(), ct);
        // Note: EF Core translates .ToLower() to LOWER() in PostgreSQL — this is safe.

    public async Task<UserProfile?> GetWithPhotosAsync(Guid userId, CancellationToken ct = default) =>
        await _context.UserProfiles
            .Include(p => p.Photos)
            .FirstOrDefaultAsync(p => p.Id == userId, ct);
}
