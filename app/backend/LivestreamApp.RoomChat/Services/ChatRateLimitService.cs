using System.Collections.Concurrent;
using System.Threading.RateLimiting;

namespace LivestreamApp.RoomChat.Services;

/// <summary>
/// In-memory sliding window rate limiter: 3 messages/second per user per room.
/// Uses System.Threading.RateLimiting — no Redis overhead for chat rate limiting.
/// </summary>
public sealed class ChatRateLimitService : IChatRateLimitService, IDisposable
{
    private readonly ConcurrentDictionary<string, SlidingWindowRateLimiter> _limiters = new();

    private static string Key(Guid userId, Guid roomId) => $"{userId}:{roomId}";

    public bool TryAcquire(Guid userId, Guid roomId)
    {
        var limiter = _limiters.GetOrAdd(Key(userId, roomId), _ => new SlidingWindowRateLimiter(
            new SlidingWindowRateLimiterOptions
            {
                PermitLimit = 3,
                Window = TimeSpan.FromSeconds(1),
                SegmentsPerWindow = 2,
                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                QueueLimit = 0
            }));

        using var lease = limiter.AttemptAcquire();
        return lease.IsAcquired;
    }

    public void Dispose()
    {
        foreach (var limiter in _limiters.Values)
            limiter.Dispose();
        _limiters.Clear();
    }
}
