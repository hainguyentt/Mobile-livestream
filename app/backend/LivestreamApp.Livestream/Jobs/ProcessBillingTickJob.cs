using LivestreamApp.Livestream.Repositories;
using LivestreamApp.Livestream.Services;
using Microsoft.Extensions.Logging;

namespace LivestreamApp.Livestream.Jobs;

/// <summary>
/// Hangfire job: deduct coins every 10 seconds per active call session.
/// Idempotent — safe to retry. Ends call on insufficient balance or 3 consecutive failures.
/// </summary>
public sealed class ProcessBillingTickJob
{
    private readonly IBillingService _billing;
    private readonly ICallSessionRepository _callRepo;
    private readonly ILogger<ProcessBillingTickJob> _logger;

    public ProcessBillingTickJob(
        IBillingService billing,
        ICallSessionRepository callRepo,
        ILogger<ProcessBillingTickJob> logger)
    {
        _billing = billing;
        _callRepo = callRepo;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid sessionId, int tickNumber, CancellationToken ct = default)
    {
        _logger.LogDebug("Processing billing tick {TickNumber} for session {SessionId}", tickNumber, sessionId);

        var result = await _billing.ProcessBillingTickAsync(sessionId, tickNumber, ct);

        switch (result)
        {
            case BillingTickResult.Success:
                _logger.LogDebug("Billing tick {TickNumber} processed for session {SessionId}", tickNumber, sessionId);
                // Caller (scheduler) is responsible for broadcasting BalanceUpdated via SignalR
                break;

            case BillingTickResult.Duplicate:
                _logger.LogDebug("Duplicate tick {TickNumber} for session {SessionId} — skipped", tickNumber, sessionId);
                break;

            case BillingTickResult.InsufficientBalance:
                _logger.LogWarning("Insufficient balance at tick {TickNumber} for session {SessionId} — ending call", tickNumber, sessionId);
                await EndCallDueToInsufficientBalanceAsync(sessionId, ct);
                break;

            case BillingTickResult.SessionEnded:
                _logger.LogDebug("Session {SessionId} already ended — skipping tick {TickNumber}", sessionId, tickNumber);
                break;

            case BillingTickResult.Error:
                _logger.LogError("Billing error at tick {TickNumber} for session {SessionId}", tickNumber, sessionId);
                throw new InvalidOperationException($"Billing tick {tickNumber} failed for session {sessionId}");
        }
    }

    private async Task EndCallDueToInsufficientBalanceAsync(Guid sessionId, CancellationToken ct)
    {
        var session = await _callRepo.GetByIdAsync(sessionId, ct);
        if (session == null || session.Status == Shared.Domain.Enums.CallSessionStatus.Ended) return;

        session.End("System");
        await _callRepo.UpdateSessionAsync(session, ct);
        // SignalR broadcast (CallEnded) is handled by domain event handler
    }
}
