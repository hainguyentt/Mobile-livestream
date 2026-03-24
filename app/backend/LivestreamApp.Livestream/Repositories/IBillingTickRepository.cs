using LivestreamApp.Livestream.Domain.Entities;

namespace LivestreamApp.Livestream.Repositories;

public interface IBillingTickRepository
{
    Task<int> GetNextTickNumberAsync(Guid callSessionId, CancellationToken ct = default);
    /// <summary>
    /// Inserts billing tick with ON CONFLICT (call_session_id, tick_number) DO NOTHING.
    /// Returns true if inserted, false if duplicate.
    /// </summary>
    Task<bool> TryInsertAsync(BillingTick tick, CancellationToken ct = default);
}
