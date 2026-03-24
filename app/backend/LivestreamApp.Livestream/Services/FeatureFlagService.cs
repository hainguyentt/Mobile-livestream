using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LivestreamApp.Livestream.Services;

public sealed class FeatureFlagService : IFeatureFlagService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<FeatureFlagService> _logger;

    private static string Key(string flagName) => $"feature_flag:{flagName}";

    public FeatureFlagService(IConnectionMultiplexer redis, ILogger<FeatureFlagService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<bool> IsEnabledAsync(string flagName, CancellationToken ct = default)
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync(Key(flagName));

            // Fail-open: null = enabled
            if (!value.HasValue) return true;
            return value.ToString() != "false";
        }
        catch (Exception ex)
        {
            // Fail-open on Redis error
            _logger.LogWarning(ex, "Failed to read feature flag {FlagName}, defaulting to enabled", flagName);
            return true;
        }
    }

    public async Task SetAsync(string flagName, bool enabled, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        await db.StringSetAsync(Key(flagName), enabled ? "true" : "false");
        _logger.LogInformation("Feature flag {FlagName} set to {Value}", flagName, enabled);
    }
}
