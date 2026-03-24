using LivestreamApp.Livestream.Repositories;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LivestreamApp.Livestream.Jobs;

/// <summary>
/// Hangfire daily job: clean up Redis keys for call sessions that ended more than 24 hours ago.
/// </summary>
public sealed class CleanupEndedCallSessionsJob
{
    private readonly ICallSessionRepository _callRepo;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CleanupEndedCallSessionsJob> _logger;

    public CleanupEndedCallSessionsJob(
        ICallSessionRepository callRepo,
        IConnectionMultiplexer redis,
        ILogger<CleanupEndedCallSessionsJob> logger)
    {
        _callRepo = callRepo;
        _redis = redis;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting cleanup of ended call session Redis keys");

        var db = _redis.GetDatabase();
        var server = _redis.GetServer(_redis.GetEndPoints().First());

        // Scan for call_session:* keys
        var pattern = "call_session:*";
        var cleaned = 0;

        await foreach (var key in server.KeysAsync(pattern: pattern))
        {
            var sessionIdStr = key.ToString().Replace("call_session:", "");
            if (!Guid.TryParse(sessionIdStr, out var sessionId)) continue;

            var session = await _callRepo.GetByIdAsync(sessionId, ct);
            if (session == null || session.Status == Shared.Domain.Enums.CallSessionStatus.Ended)
            {
                await db.KeyDeleteAsync(key);
                cleaned++;
            }
        }

        _logger.LogInformation("Cleaned up {Count} ended call session Redis keys", cleaned);
    }
}
