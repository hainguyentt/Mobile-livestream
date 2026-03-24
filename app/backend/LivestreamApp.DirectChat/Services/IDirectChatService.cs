using LivestreamApp.DirectChat.Domain.Entities;
using LivestreamApp.Shared.Domain.Enums;

namespace LivestreamApp.DirectChat.Services;

public interface IDirectChatService
{
    Task<Conversation> GetOrCreateConversationAsync(Guid viewerId, Guid hostId, CancellationToken ct = default);
    Task<List<Conversation>> GetConversationsAsync(Guid userId, CancellationToken ct = default);
    Task<Conversation?> GetConversationByIdAsync(Guid conversationId, Guid userId, CancellationToken ct = default);

    /// <summary>
    /// Returns messages in a conversation. 'from' is required to prevent full-table scans on partitioned table.
    /// </summary>
    Task<List<DirectMessage>> GetMessagesAsync(Guid conversationId, Guid userId, DateTime from, DateTime? to = null, CancellationToken ct = default);

    Task<DirectMessage> SendMessageAsync(Guid conversationId, Guid senderId, string content,
        MessageType messageType = MessageType.Text, string? emojiCode = null, CancellationToken ct = default);

    Task MarkAsReadAsync(Guid conversationId, Guid userId, CancellationToken ct = default);
    Task BlockUserAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default);
    Task UnblockUserAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default);
    Task<bool> IsBlockedAsync(Guid userId1, Guid userId2, CancellationToken ct = default);
}
