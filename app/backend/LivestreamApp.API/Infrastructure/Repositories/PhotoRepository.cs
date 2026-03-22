using LivestreamApp.Profiles.Domain.Entities;
using LivestreamApp.Profiles.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Repositories;

/// <summary>EF Core implementation of IPhotoRepository.</summary>
public class PhotoRepository : IPhotoRepository
{
    private readonly AppDbContext _context;

    public PhotoRepository(AppDbContext context) => _context = context;

    public async Task<UserPhoto?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
        await _context.UserPhotos.FindAsync([id], ct);

    public async Task<IReadOnlyList<UserPhoto>> GetAllAsync(CancellationToken ct = default) =>
        await _context.UserPhotos.ToListAsync(ct);

    public async Task AddAsync(UserPhoto entity, CancellationToken ct = default) =>
        await _context.UserPhotos.AddAsync(entity, ct);

    public void Update(UserPhoto entity) => _context.UserPhotos.Update(entity);

    public void Remove(UserPhoto entity) => _context.UserPhotos.Remove(entity);

    public async Task<IReadOnlyList<UserPhoto>> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await _context.UserPhotos
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.DisplayIndex)
            .ToListAsync(ct);

    public async Task<UserPhoto?> GetByIdAndUserIdAsync(Guid photoId, Guid userId, CancellationToken ct = default) =>
        await _context.UserPhotos.FirstOrDefaultAsync(p => p.Id == photoId && p.UserId == userId, ct);

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken ct = default) =>
        await _context.UserPhotos.CountAsync(p => p.UserId == userId, ct);

    public async Task<UserPhoto?> GetByUserIdAndIndexAsync(Guid userId, int displayIndex, CancellationToken ct = default) =>
        await _context.UserPhotos.FirstOrDefaultAsync(p => p.UserId == userId && p.DisplayIndex == displayIndex, ct);
}
