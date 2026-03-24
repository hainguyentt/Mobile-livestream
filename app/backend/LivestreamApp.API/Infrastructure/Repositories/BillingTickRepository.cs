using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Livestream.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LivestreamApp.API.Infrastructure.Repositories;

public sealed class BillingTickRepository : IBillingTickRepository
{
    private readonly AppDbContext _db;
    public BillingTickRepository(AppDbContext db) => _db = db;

    public Task<int> GetNextTickNumberAsync(Guid callSessionId, CancellationToken ct = default)
        => _db.BillingTicks
            .Where(t => t.CallSessionId == callSessionId)
            .Select(t => t.TickNumber)
            .DefaultIfEmpty(0)
            .MaxAsync(ct)
            .ContinueWith(t => t.Result + 1, ct);

    public async Task<bool> TryInsertAsync(BillingTick tick, CancellationToken ct = default)
    {
        try
        {
            await _db.BillingTicks.AddAsync(tick, ct);
            await _db.SaveChangesAsync(ct);
            return true;
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("unique") == true
            || ex.InnerException?.Message.Contains("duplicate") == true)
        {
            // ON CONFLICT DO NOTHING — duplicate tick
            _db.ChangeTracker.Clear();
            return false;
        }
    }
}
