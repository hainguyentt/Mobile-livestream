using LivestreamApp.DirectChat.Domain.Entities;

namespace LivestreamApp.API.DTOs.DirectChat;

public sealed record ConversationResponse(
    Guid Id,
    Guid OtherUserId,
    string? LastMessagePreview,
    DateTime? LastMessageAt,
    int UnreadCount,
    DateTime CreatedAt)
{
    public static ConversationResponse From(Conversation conversation, Guid currentUserId)
    {
        var otherUserId = conversation.ViewerId == currentUserId ? conversation.HostId : conversation.ViewerId;
        var unreadCount = conversation.ViewerId == currentUserId
            ? conversation.ViewerUnreadCount
            : conversation.HostUnreadCount;

        return new ConversationResponse(
            conversation.Id,
            otherUserId,
            conversation.LastMessagePreview,
            conversation.LastMessageAt,
            unreadCount,
            conversation.CreatedAt);
    }
}
