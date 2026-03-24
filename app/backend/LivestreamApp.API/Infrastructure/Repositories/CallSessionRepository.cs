using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Livestream.Repositories;
using LivestreamApp.Shared.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Repositories;

public sealed class CallSessionRepository : ICallSessionRepository
{
    private readonly AppDbContext _db;
    public CallSessionRepository(AppDbContext db) => _db = db;

    public Task<CallSession?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.CallSessions.Include(s => s.BillingTicks).FirstOrDefaultAsync(s => s.Id == id, ct);

    public Task<CallSession?> GetActiveSessionByHostAsync(Guid hostId, CancellationToken ct = default)
        => _db.CallSessions.FirstOrDefaultAsync(
            s => s.HostId == hostId && s.Status == CallSessionStatus.Active, ct);

    public Task<List<CallSession>> GetActiveSessionsAsync(CancellationToken ct = default)
        => _db.CallSessions.Where(s => s.Status == CallSessionStatus.Active).ToListAsync(ct);

    public Task<PrivateCallRequest?> GetPendingRequestByHostAsync(Guid hostId, CancellationToken ct = default)
        => _db.PrivateCallRequests.FirstOrDefaultAsync(
            r => r.HostId == hostId && r.Status == CallRequestStatus.Pending, ct);

    public Task<PrivateCallRequest?> GetRequestByIdAsync(Guid requestId, CancellationToken ct = default)
        => _db.PrivateCallRequests.FirstOrDefaultAsync(r => r.Id == requestId, ct);

    public async Task AddRequestAsync(PrivateCallRequest request, CancellationToken ct = default)
        => await _db.PrivateCallRequests.AddAsync(request, ct);

    public Task UpdateRequestAsync(PrivateCallRequest request, CancellationToken ct = default)
    {
        _db.PrivateCallRequests.Update(request);
        return Task.CompletedTask;
    }

    public async Task AddSessionAsync(CallSession session, CancellationToken ct = default)
        => await _db.CallSessions.AddAsync(session, ct);

    public Task UpdateSessionAsync(CallSession session, CancellationToken ct = default)
    {
        _db.CallSessions.Update(session);
        return Task.CompletedTask;
    }
}
