using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Livestream.Repositories;
using LivestreamApp.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Repositories;

public sealed class LivestreamRoomRepository : ILivestreamRoomRepository
{
    private readonly AppDbContext _db;
    public LivestreamRoomRepository(AppDbContext db) => _db = db;

    public Task<LivestreamRoom?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.LivestreamRooms.Include(r => r.ViewerSessions).Include(r => r.KickedViewers)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public Task<LivestreamRoom?> GetActiveRoomByHostAsync(Guid hostId, CancellationToken ct = default)
        => _db.LivestreamRooms.FirstOrDefaultAsync(
            r => r.HostId == hostId && r.Status == RoomStatus.Live, ct);

    public Task<List<LivestreamRoom>> GetActiveRoomsAsync(RoomCategory? category, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _db.LivestreamRooms.Where(r => r.Status == RoomStatus.Live);
        if (category.HasValue) query = query.Where(r => r.Category == category.Value);
        return query.OrderByDescending(r => r.ViewerCount)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
    }

    public Task<int> CountActiveRoomsAsync(RoomCategory? category, CancellationToken ct = default)
    {
        var query = _db.LivestreamRooms.Where(r => r.Status == RoomStatus.Live);
        if (category.HasValue) query = query.Where(r => r.Category == category.Value);
        return query.CountAsync(ct);
    }

    public async Task AddAsync(LivestreamRoom room, CancellationToken ct = default)
        => await _db.LivestreamRooms.AddAsync(room, ct);

    public Task UpdateAsync(LivestreamRoom room, CancellationToken ct = default)
    {
        _db.LivestreamRooms.Update(room);
        return Task.CompletedTask;
    }
}
