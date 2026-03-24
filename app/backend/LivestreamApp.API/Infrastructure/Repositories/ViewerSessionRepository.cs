using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Livestream.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Repositories;

public sealed class ViewerSessionRepository : IViewerSessionRepository
{
    private readonly AppDbContext _db;
    public ViewerSessionRepository(AppDbContext db) => _db = db;

    public Task<ViewerSession?> GetActiveSessionAsync(Guid roomId, Guid viewerId, CancellationToken ct = default)
        => _db.ViewerSessions.FirstOrDefaultAsync(
            s => s.RoomId == roomId && s.ViewerId == viewerId && s.LeftAt == null, ct);

    public Task<int> CountActiveViewersAsync(Guid roomId, CancellationToken ct = default)
        => _db.ViewerSessions.CountAsync(s => s.RoomId == roomId && s.LeftAt == null, ct);

    public Task<bool> IsViewerKickedAsync(Guid roomId, Guid viewerId, CancellationToken ct = default)
        => _db.KickedViewers.AnyAsync(k => k.RoomId == roomId && k.ViewerId == viewerId, ct);

    public async Task AddAsync(ViewerSession session, CancellationToken ct = default)
        => await _db.ViewerSessions.AddAsync(session, ct);

    public Task UpdateAsync(ViewerSession session, CancellationToken ct = default)
    {
        _db.ViewerSessions.Update(session);
        return Task.CompletedTask;
    }
}
