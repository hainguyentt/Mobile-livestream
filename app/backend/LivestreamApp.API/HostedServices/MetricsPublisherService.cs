using LivestreamApp.API.Hubs;
using LivestreamApp.Livestream.Services;
using Microsoft.Extensions.Configuration;

namespace LivestreamApp.API.HostedServices;

/// <summary>
/// Background service: publishes custom CloudWatch metrics every 30 seconds.
/// Metrics: SignalR.ConnectionCount, SignalR.ActiveRooms, Agora.UsageMinutes.
/// </summary>
public sealed class MetricsPublisherService : BackgroundService
{
    private readonly IConnectionTracker _tracker;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<MetricsPublisherService> _logger;

    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(30);

    public MetricsPublisherService(
        IConnectionTracker tracker,
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<MetricsPublisherService> logger)
    {
        _tracker = tracker;
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            try
            {
                await PublishMetricsAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Error publishing metrics");
            }
        }
    }

    private async Task PublishMetricsAsync(CancellationToken ct)
    {
        var connectionCount = _tracker.GetConnectionCount();
        var activeRooms = _tracker.GetActiveRoomIds().Count();

        using var scope = _scopeFactory.CreateScope();
        var agora = scope.ServiceProvider.GetRequiredService<IAgoraTokenService>();
        var usageMinutes = await agora.GetCurrentMonthUsageMinutesAsync(ct);

        // Log metrics — CloudWatch agent picks these up via structured logging
        _logger.LogInformation(
            "Metrics: SignalR.ConnectionCount={ConnectionCount}, SignalR.ActiveRooms={ActiveRooms}, Agora.UsageMinutes={UsageMinutes}",
            connectionCount, activeRooms, usageMinutes);

        // TODO: publish to CloudWatch via Amazon.CloudWatch SDK when deployed to AWS
        // PutMetricData(Namespace="LivestreamApp/SignalR", MetricName="ConnectionCount", Value=connectionCount)
    }
}
