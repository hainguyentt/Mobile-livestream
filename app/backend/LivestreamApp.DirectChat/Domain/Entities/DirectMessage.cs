using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Domain.Primitives;
using LivestreamApp.Shared.Exceptions;

namespace LivestreamApp.DirectChat.Domain.Entities;

/// <summary>
/// Direct message in a 1-1 conversation.
/// Stored in PostgreSQL table partitioned by sent_at (monthly).
/// </summary>
public sealed class DirectMessage : Entity<Guid>
{
    public Guid ConversationId { get; private set; }
    public Guid SenderId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public MessageType MessageType { get; private set; }
    public string? EmojiCode { get; private set; }
    public bool IsRead { get; private set; }
    public DateTime? ReadAt { get; private set; }
    public bool IsDeletedBySender { get; private set; }
    public DateTime SentAt { get; private set; }  // Partition key

    private DirectMessage(Guid id, Guid conversationId, Guid senderId, string content,
        MessageType messageType, string? emojiCode) : base(id)
    {
        ConversationId = conversationId;
        SenderId = senderId;
        Content = content;
        MessageType = messageType;
        EmojiCode = emojiCode;
        SentAt = DateTime.UtcNow;
    }

    // EF Core constructor
    private DirectMessage() : base(Guid.Empty) { }

    public static DirectMessage Create(Guid conversationId, Guid senderId, string content,
        MessageType messageType = MessageType.Text, string? emojiCode = null)
    {
        if (conversationId == Guid.Empty) throw new DomainException("ConversationId is required.");
        if (senderId == Guid.Empty) throw new DomainException("SenderId is required.");
        if (string.IsNullOrWhiteSpace(content)) throw new DomainException("Content is required.");
        if (content.Length > 1000) throw new DomainException("Content must not exceed 1000 characters.");
        if (messageType == MessageType.Emoji && string.IsNullOrWhiteSpace(emojiCode))
            throw new DomainException("EmojiCode is required for emoji messages.");

        return new DirectMessage(Guid.NewGuid(), conversationId, senderId, content, messageType, emojiCode);
    }

    public void MarkAsRead()
    {
        if (IsRead) return;
        IsRead = true;
        ReadAt = DateTime.UtcNow;
    }

    public void SoftDeleteBySender()
    {
        IsDeletedBySender = true;
    }
}
