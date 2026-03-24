using LivestreamApp.API.Hubs;
using LivestreamApp.Livestream.Services;
using Microsoft.AspNetCore.SignalR;

namespace LivestreamApp.API.HostedServices;

/// <summary>
/// Background service: broadcasts viewer counts to active room groups every 5 seconds.
/// Only broadcasts when count changes (delta check).
/// </summary>
public sealed class ViewerCountBroadcastService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHubContext<LivestreamHub> _hub;
    private readonly IConnectionTracker _tracker;
    private readonly ILogger<ViewerCountBroadcastService> _logger;

    private readonly Dictionary<Guid, long> _lastCounts = new();
    private static readonly TimeSpan Interval = TimeSpan.FromSeconds(5);

    public ViewerCountBroadcastService(
        IServiceScopeFactory scopeFactory,
        IHubContext<LivestreamHub> hub,
        IConnectionTracker tracker,
        ILogger<ViewerCountBroadcastService> logger)
    {
        _scopeFactory = scopeFactory;
        _hub = hub;
        _tracker = tracker;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(Interval, stoppingToken);

            try
            {
                await BroadcastViewerCountsAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogWarning(ex, "Error broadcasting viewer counts");
            }
        }
    }

    private async Task BroadcastViewerCountsAsync(CancellationToken ct)
    {
        var activeRoomIds = _tracker.GetActiveRoomIds().ToList();
        if (activeRoomIds.Count == 0) return;

        using var scope = _scopeFactory.CreateScope();
        var viewerCount = scope.ServiceProvider.GetRequiredService<IViewerCountService>();

        var counts = await viewerCount.GetCountsAsync(activeRoomIds, ct);

        foreach (var (roomId, count) in counts)
        {
            // Only broadcast if count changed
            if (_lastCounts.TryGetValue(roomId, out var last) && last == count) continue;

            _lastCounts[roomId] = count;
            await _hub.Clients.Group($"room:{roomId}")
                .SendAsync("ViewerCountUpdated", new { roomId, viewerCount = count }, ct);
        }
    }
}
