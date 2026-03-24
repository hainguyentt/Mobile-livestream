using LivestreamApp.Livestream.Repositories;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LivestreamApp.Livestream.Services;

public sealed class ViewerCountService : IViewerCountService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IViewerSessionRepository _sessions;
    private readonly ILogger<ViewerCountService> _logger;

    private static string Key(Guid roomId) => $"viewer_count:{roomId}";

    public ViewerCountService(
        IConnectionMultiplexer redis,
        IViewerSessionRepository sessions,
        ILogger<ViewerCountService> logger)
    {
        _redis = redis;
        _sessions = sessions;
        _logger = logger;
    }

    public async Task<long> IncrementAsync(Guid roomId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        return await db.StringIncrementAsync(Key(roomId));
    }

    public async Task<long> DecrementAsync(Guid roomId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var result = await db.StringDecrementAsync(Key(roomId));
        if (result < 0)
        {
            await db.StringSetAsync(Key(roomId), 0);
            return 0;
        }
        return result;
    }

    public async Task<long> GetCountAsync(Guid roomId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var value = await db.StringGetAsync(Key(roomId));

        if (value.HasValue && long.TryParse(value, out var count))
            return count;

        // Lazy resync from DB on cache miss
        _logger.LogDebug("Cache miss for viewer count of room {RoomId}, resyncing from DB", roomId);
        var dbCount = await _sessions.CountActiveViewersAsync(roomId, ct);
        await db.StringSetAsync(Key(roomId), dbCount, TimeSpan.FromHours(1));
        return dbCount;
    }

    public async Task<Dictionary<Guid, long>> GetCountsAsync(IEnumerable<Guid> roomIds, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var ids = roomIds.ToList();
        var keys = ids.Select(id => (RedisKey)Key(id)).ToArray();
        var values = await db.StringGetAsync(keys);

        var result = new Dictionary<Guid, long>();
        for (var i = 0; i < ids.Count; i++)
        {
            result[ids[i]] = values[i].HasValue && long.TryParse(values[i], out var c) ? c : 0;
        }
        return result;
    }

    public async Task ResetAsync(Guid roomId, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        await db.KeyDeleteAsync(Key(roomId));
    }
}
