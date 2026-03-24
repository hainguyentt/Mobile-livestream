namespace LivestreamApp.Livestream.Services;

public interface IFeatureFlagService
{
    /// <summary>Returns true if feature is enabled. Defaults to true (fail-open) when flag not set.</summary>
    Task<bool> IsEnabledAsync(string flagName, CancellationToken ct = default);
    Task SetAsync(string flagName, bool enabled, CancellationToken ct = default);
}
