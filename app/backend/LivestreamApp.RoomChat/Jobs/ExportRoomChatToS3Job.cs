using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System.Text;
using System.Text.Json;

namespace LivestreamApp.RoomChat.Jobs;

/// <summary>
/// Hangfire daily job (02:00 UTC): export room chat from Redis Streams to S3 as JSON Lines.
/// Failed exports go to Hangfire Dead Letter Queue for manual retry.
/// </summary>
public sealed class ExportRoomChatToS3Job
{
    private readonly IConnectionMultiplexer _redis;
    private readonly IAmazonS3 _s3;
    private readonly string _bucketName;
    private readonly ILogger<ExportRoomChatToS3Job> _logger;

    public ExportRoomChatToS3Job(
        IConnectionMultiplexer redis,
        IAmazonS3 s3,
        Microsoft.Extensions.Configuration.IConfiguration config,
        ILogger<ExportRoomChatToS3Job> logger)
    {
        _redis = redis;
        _s3 = s3;
        _bucketName = config["S3:BucketName"] ?? "livestream-photos";
        _logger = logger;
    }

    public async Task ExecuteAsync(CancellationToken ct = default)
    {
        _logger.LogInformation("Starting room chat S3 export job");

        var server = _redis.GetServer(_redis.GetEndPoints().First());
        var today = DateTime.UtcNow.Date;
        var exported = 0;
        var failed = 0;

        await foreach (var key in server.KeysAsync(pattern: "room:*:chat"))
        {
            var keyStr = key.ToString();
            var roomIdStr = keyStr.Replace("room:", "").Replace(":chat", "");
            if (!Guid.TryParse(roomIdStr, out var roomId)) continue;

            try
            {
                await ExportRoomAsync(roomId, today, ct);
                exported++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to export chat for room {RoomId}", roomId);
                failed++;
                // Hangfire will retry based on job retry policy
                throw; // Re-throw to trigger Hangfire retry/dead-letter
            }
        }

        _logger.LogInformation("Chat export complete: {Exported} rooms exported, {Failed} failed", exported, failed);
    }

    private async Task ExportRoomAsync(Guid roomId, DateTime date, CancellationToken ct)
    {
        var db = _redis.GetDatabase();
        var key = $"room:{roomId}:chat";

        var entries = await db.StreamRangeAsync(key, "-", "+");
        if (entries.Length == 0) return;

        var sb = new StringBuilder();
        foreach (var entry in entries)
        {
            var fields = entry.Values.ToDictionary(v => v.Name.ToString(), v => v.Value.ToString());
            var line = JsonSerializer.Serialize(new
            {
                id = entry.Id.ToString(),
                userId = fields.GetValueOrDefault("userId"),
                displayName = fields.GetValueOrDefault("displayName"),
                text = fields.GetValueOrDefault("text"),
                timestamp = fields.GetValueOrDefault("timestamp"),
                type = fields.GetValueOrDefault("type"),
                giftId = fields.GetValueOrDefault("giftId")
            });
            sb.AppendLine(line);
        }

        var s3Key = $"chat-archive/{date:yyyy}/{date:MM}/{roomId}/{date:yyyy-MM-dd}.jsonl";
        var content = Encoding.UTF8.GetBytes(sb.ToString());

        await _s3.PutObjectAsync(new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = s3Key,
            InputStream = new System.IO.MemoryStream(content),
            ContentType = "application/x-ndjson"
        });
        _logger.LogInformation("Exported {Count} messages for room {RoomId} to {S3Key}", entries.Length, roomId, s3Key);
    }
}
