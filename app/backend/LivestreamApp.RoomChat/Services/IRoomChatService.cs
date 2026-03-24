using LivestreamApp.RoomChat.Domain;

namespace LivestreamApp.RoomChat.Services;

public interface IRoomChatService
{
    /// <summary>Sends a message to the room chat stream. Applies profanity filter and rate limit check.</summary>
    Task<RoomChatMessage> SendMessageAsync(Guid roomId, Guid senderId, string senderDisplayName, string content, CancellationToken ct = default);

    /// <summary>Sends a gift event to the room chat stream.</summary>
    Task<RoomChatMessage> SendGiftAsync(Guid roomId, Guid senderId, string senderDisplayName, string giftId, CancellationToken ct = default);

    /// <summary>Returns the last N messages from the room chat stream.</summary>
    Task<List<RoomChatMessage>> GetRecentMessagesAsync(Guid roomId, int count = 50, CancellationToken ct = default);
}
