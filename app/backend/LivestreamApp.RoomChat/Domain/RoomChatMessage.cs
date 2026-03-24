namespace LivestreamApp.RoomChat.Domain;

/// <summary>
/// Room chat message stored in Redis Stream.
/// Not a DB entity — serialized to/from Redis Stream fields.
/// </summary>
public sealed record RoomChatMessage
{
    public string MessageId { get; init; } = string.Empty;  // Redis Stream entry ID
    public Guid RoomId { get; init; }
    public Guid SenderId { get; init; }
    public string SenderDisplayName { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;    // Max 200 chars, profanity-filtered
    public string Type { get; init; } = "message";          // message | gift | system
    public string? GiftId { get; init; }                    // Set when Type = gift
    public DateTime SentAt { get; init; }
}
