using LivestreamApp.Livestream.Services;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LivestreamApp.Livestream.Jobs;

/// <summary>
/// Hangfire daily job: check Agora usage quota.
/// Disables private-call feature flag at 90% (9,000 min), alerts at 80% (8,000 min).
/// </summary>
public sealed class CheckAgoraQuotaJob
{
    private const long QuotaLimit = 10_000;
    private const long DisableThreshold = 9_000;  // 90%
    private const long AlertThreshold = 8_000;    // 80%

    private readonly IAgoraTokenService _agora;
    private readonly IFeatureFlagService _featureFlags;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CheckAgoraQuotaJob> _logger;

    public CheckAgoraQuotaJob(
        IAgoraTokenService agora,
        IFeatureFlagService featureFlags,
        IConnectionMultiplexer redis,
        ILogger<CheckAgoraQuotaJob> logger)
    {
        _agora = agora;
        _featureFlags = featureFlags;
        _redis = redis;
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        var usageMinutes = await _agora.GetCurrentMonthUsageMinutesAsync(ct);
        _logger.LogInformation("Agora usage this month: {Minutes} minutes", usageMinutes);

        // Cache quota in Redis for MetricsPublisherService
        var db = _redis.GetDatabase();
        var secondsToMonthEnd = GetSecondsToMonthEnd();
        await db.StringSetAsync("agora_quota:current_month", usageMinutes, TimeSpan.FromSeconds(secondsToMonthEnd));

        if (usageMinutes >= DisableThreshold)
        {
            await _featureFlags.SetAsync("private-call", false, ct);
            _logger.LogCritical("Agora quota critical ({Minutes}/{Limit} min) — private-call DISABLED", usageMinutes, QuotaLimit);
        }
        else if (usageMinutes >= AlertThreshold)
        {
            _logger.LogWarning("Agora quota warning ({Minutes}/{Limit} min) — approaching limit", usageMinutes, QuotaLimit);
            // Re-enable if previously disabled and now below threshold
            var isEnabled = await _featureFlags.IsEnabledAsync("private-call", ct);
            if (!isEnabled)
            {
                await _featureFlags.SetAsync("private-call", true, ct);
                _logger.LogInformation("Agora quota below disable threshold — private-call RE-ENABLED");
            }
        }
    }

    private static long GetSecondsToMonthEnd()
    {
        var now = DateTime.UtcNow;
        var nextMonth = new DateTime(now.Year, now.Month, 1).AddMonths(1);
        return (long)(nextMonth - now).TotalSeconds;
    }
}
