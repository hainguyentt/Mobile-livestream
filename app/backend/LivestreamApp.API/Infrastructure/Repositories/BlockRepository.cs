using LivestreamApp.DirectChat.Domain.Entities;
using LivestreamApp.DirectChat.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Repositories;

public sealed class BlockRepository : IBlockRepository
{
    private readonly AppDbContext _db;
    public BlockRepository(AppDbContext db) => _db = db;

    public Task<bool> ExistsAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default)
        => _db.Blocks.AnyAsync(b => b.BlockerId == blockerId && b.BlockedId == blockedId, ct);

    public Task<bool> ExistsInEitherDirectionAsync(Guid userId1, Guid userId2, CancellationToken ct = default)
        => _db.Blocks.AnyAsync(b =>
            (b.BlockerId == userId1 && b.BlockedId == userId2) ||
            (b.BlockerId == userId2 && b.BlockedId == userId1), ct);

    public async Task AddAsync(Block block, CancellationToken ct = default)
        => await _db.Blocks.AddAsync(block, ct);

    public async Task DeleteAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default)
    {
        var block = await _db.Blocks.FirstOrDefaultAsync(
            b => b.BlockerId == blockerId && b.BlockedId == blockedId, ct);
        if (block != null) _db.Blocks.Remove(block);
    }
}
