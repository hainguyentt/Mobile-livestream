using LivestreamApp.Livestream.Domain.Entities;
using LivestreamApp.Livestream.Repositories;
using LivestreamApp.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace LivestreamApp.Livestream.Services;

public sealed class BillingService : IBillingService
{
    private readonly ICallSessionRepository _callRepo;
    private readonly IBillingTickRepository _billingTicks;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<BillingService> _logger;

    // Coin wallet operations — delegated to a wallet service (Unit 1 dependency)
    // For now, use a simple in-memory stub; replace with real wallet service
    private readonly ICoinWalletService _wallet;

    public BillingService(
        ICallSessionRepository callRepo,
        IBillingTickRepository billingTicks,
        ICoinWalletService wallet,
        IUnitOfWork uow,
        ILogger<BillingService> logger)
    {
        _callRepo = callRepo;
        _billingTicks = billingTicks;
        _wallet = wallet;
        _uow = uow;
        _logger = logger;
    }

    public async Task<BillingTickResult> ProcessBillingTickAsync(Guid sessionId, int tickNumber, CancellationToken ct = default)
    {
        var session = await _callRepo.GetByIdAsync(sessionId, ct);
        if (session == null || session.Status == Shared.Domain.Enums.CallSessionStatus.Ended)
            return BillingTickResult.SessionEnded;

        var balance = await _wallet.GetBalanceAsync(session.ViewerId, ct);
        if (balance < session.CoinRatePerTick)
        {
            _logger.LogWarning("Insufficient balance for session {SessionId} tick {TickNumber}", sessionId, tickNumber);
            return BillingTickResult.InsufficientBalance;
        }

        var tick = BillingTick.Create(
            sessionId, tickNumber, session.CoinRatePerTick,
            balance, balance - session.CoinRatePerTick, isSuccess: true);

        // Idempotent insert — ON CONFLICT DO NOTHING
        var inserted = await _billingTicks.TryInsertAsync(tick, ct);
        if (!inserted)
        {
            _logger.LogDebug("Duplicate billing tick {TickNumber} for session {SessionId}", tickNumber, sessionId);
            return BillingTickResult.Duplicate;
        }

        await _wallet.DeductAsync(session.ViewerId, session.CoinRatePerTick, ct);
        session.RecordBillingTick(session.CoinRatePerTick);
        await _callRepo.UpdateSessionAsync(session, ct);
        await _uow.SaveChangesAsync(ct);

        return BillingTickResult.Success;
    }

    public Task<int> GetBalanceAsync(Guid userId, CancellationToken ct = default)
        => _wallet.GetBalanceAsync(userId, ct);

    public async Task<bool> CheckSufficientBalanceAsync(Guid userId, int requiredCoins, CancellationToken ct = default)
    {
        var balance = await _wallet.GetBalanceAsync(userId, ct);
        return balance >= requiredCoins;
    }

    public Task FinalizeCallBillingAsync(Guid sessionId, CancellationToken ct = default)
    {
        // Finalization: cancel pending Hangfire jobs, update session totals
        // Hangfire job cancellation is handled by the job itself checking session status
        _logger.LogInformation("Billing finalized for session {SessionId}", sessionId);
        return Task.CompletedTask;
    }
}
