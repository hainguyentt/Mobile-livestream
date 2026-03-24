namespace LivestreamApp.Livestream.Services;

public interface IBillingService
{
    /// <summary>Processes a single billing tick. Idempotent — safe to retry.</summary>
    Task<BillingTickResult> ProcessBillingTickAsync(Guid sessionId, int tickNumber, CancellationToken ct = default);
    Task<int> GetBalanceAsync(Guid userId, CancellationToken ct = default);
    Task<bool> CheckSufficientBalanceAsync(Guid userId, int requiredCoins, CancellationToken ct = default);
    Task FinalizeCallBillingAsync(Guid sessionId, CancellationToken ct = default);
}

public enum BillingTickResult
{
    Success,
    Duplicate,
    InsufficientBalance,
    SessionEnded,
    Error
}
