using LivestreamApp.Livestream.Repositories;
using Microsoft.Extensions.Logging;

namespace LivestreamApp.Livestream.Jobs;

/// <summary>
/// Hangfire job: auto-reject call requests that have not been responded to within 30 seconds.
/// Scheduled when a call request is created.
/// </summary>
public sealed class AutoRejectCallRequestJob
{
    private readonly ICallSessionRepository _callRepo;
    private readonly ILogger<AutoRejectCallRequestJob> _logger;

    public AutoRejectCallRequestJob(
        ICallSessionRepository callRepo,
        ILogger<AutoRejectCallRequestJob> logger)
    {
        _callRepo = callRepo;
        _logger = logger;
    }

    public async Task ExecuteAsync(Guid requestId, CancellationToken ct = default)
    {
        var request = await _callRepo.GetRequestByIdAsync(requestId, ct);
        if (request == null)
        {
            _logger.LogDebug("Call request {RequestId} not found — skipping auto-reject", requestId);
            return;
        }

        if (request.Status != Shared.Domain.Enums.CallRequestStatus.Pending)
        {
            _logger.LogDebug("Call request {RequestId} already in status {Status} — skipping", requestId, request.Status);
            return;
        }

        request.MarkTimedOut();
        await _callRepo.UpdateRequestAsync(request, ct);

        _logger.LogInformation("Call request {RequestId} auto-rejected (timed out)", requestId);
        // SignalR notification to viewer is handled by domain event handler
    }
}
