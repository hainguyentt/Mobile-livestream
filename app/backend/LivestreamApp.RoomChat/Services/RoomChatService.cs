using LivestreamApp.RoomChat.Domain;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace LivestreamApp.RoomChat.Services;

public sealed class RoomChatService : IRoomChatService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IChatRateLimitService _rateLimit;
    private readonly ILogger<RoomChatService> _logger;

    private const int MaxContentLength = 200;
    private const int StreamMaxLength = 1000;
    private static readonly TimeSpan StreamTtl = TimeSpan.FromDays(7);

    private static string StreamKey(Guid roomId) => $"room:{roomId}:chat";

    public RoomChatService(
        IConnectionMultiplexer redis,
        IChatRateLimitService rateLimit,
        ILogger<RoomChatService> logger)
    {
        _redis = redis;
        _rateLimit = rateLimit;
        _logger = logger;
    }

    public async Task<RoomChatMessage> SendMessageAsync(
        Guid roomId, Guid senderId, string senderDisplayName, string content, CancellationToken ct = default)
    {
        // Rate limit: 3 messages/second per user per room
        if (!_rateLimit.TryAcquire(senderId, roomId))
            throw new InvalidOperationException("Rate limit exceeded. Please slow down.");

        // Truncate and apply basic profanity filter
        var filteredContent = ApplyProfanityFilter(content.Length > MaxContentLength
            ? content[..MaxContentLength]
            : content);

        return await AppendToStreamAsync(roomId, senderId, senderDisplayName, filteredContent, "message", null);
    }

    public Task<RoomChatMessage> SendGiftAsync(
        Guid roomId, Guid senderId, string senderDisplayName, string giftId, CancellationToken ct = default)
        => AppendToStreamAsync(roomId, senderId, senderDisplayName, $"sent a gift: {giftId}", "gift", giftId);

    public async Task<List<RoomChatMessage>> GetRecentMessagesAsync(Guid roomId, int count = 50, CancellationToken ct = default)
    {
        var db = _redis.GetDatabase();
        var entries = await db.StreamRangeAsync(StreamKey(roomId), "-", "+", count: count, messageOrder: Order.Descending);

        return entries.Reverse().Select(e => ParseEntry(roomId, e)).ToList();
    }

    private async Task<RoomChatMessage> AppendToStreamAsync(
        Guid roomId, Guid senderId, string senderDisplayName, string content, string type, string? giftId)
    {
        var db = _redis.GetDatabase();
        var key = StreamKey(roomId);

        var fields = new NameValueEntry[]
        {
            new("userId", senderId.ToString()),
            new("displayName", senderDisplayName),
            new("text", content),
            new("timestamp", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString()),
            new("type", type),
            new("giftId", giftId ?? string.Empty)
        };

        var entryId = await db.StreamAddAsync(key, fields, maxLength: StreamMaxLength, useApproximateMaxLength: true);

        // Refresh TTL on each message
        await db.KeyExpireAsync(key, StreamTtl);

        var message = new RoomChatMessage
        {
            MessageId = entryId.ToString(),
            RoomId = roomId,
            SenderId = senderId,
            SenderDisplayName = senderDisplayName,
            Content = content,
            Type = type,
            GiftId = string.IsNullOrEmpty(giftId) ? null : giftId,
            SentAt = DateTime.UtcNow
        };

        _logger.LogDebug("Message {MessageId} added to room {RoomId} stream", entryId, roomId);
        return message;
    }

    private static RoomChatMessage ParseEntry(Guid roomId, StreamEntry entry)
    {
        var fields = entry.Values.ToDictionary(v => v.Name.ToString(), v => v.Value.ToString());
        return new RoomChatMessage
        {
            MessageId = entry.Id.ToString(),
            RoomId = roomId,
            SenderId = Guid.TryParse(fields.GetValueOrDefault("userId"), out var uid) ? uid : Guid.Empty,
            SenderDisplayName = fields.GetValueOrDefault("displayName") ?? string.Empty,
            Content = fields.GetValueOrDefault("text") ?? string.Empty,
            Type = fields.GetValueOrDefault("type") ?? "message",
            GiftId = fields.GetValueOrDefault("giftId") is { Length: > 0 } g ? g : null,
            SentAt = long.TryParse(fields.GetValueOrDefault("timestamp"), out var ts)
                ? DateTimeOffset.FromUnixTimeMilliseconds(ts).UtcDateTime
                : DateTime.UtcNow
        };
    }

    private static string ApplyProfanityFilter(string content)
    {
        // TODO: integrate real profanity filter library (e.g., ProfanityDetector)
        return content;
    }
}
