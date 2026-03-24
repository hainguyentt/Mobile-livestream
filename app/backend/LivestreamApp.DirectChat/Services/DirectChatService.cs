using LivestreamApp.DirectChat.Domain.Entities;
using LivestreamApp.DirectChat.Repositories;
using LivestreamApp.Shared.Domain.Enums;
using LivestreamApp.Shared.Exceptions;
using LivestreamApp.Shared.Interfaces;
using Microsoft.Extensions.Logging;

namespace LivestreamApp.DirectChat.Services;

public sealed class DirectChatService : IDirectChatService
{
    private readonly IConversationRepository _conversations;
    private readonly IDirectMessageRepository _messages;
    private readonly IBlockRepository _blocks;
    private readonly IUnitOfWork _uow;
    private readonly ILogger<DirectChatService> _logger;

    public DirectChatService(
        IConversationRepository conversations,
        IDirectMessageRepository messages,
        IBlockRepository blocks,
        IUnitOfWork uow,
        ILogger<DirectChatService> logger)
    {
        _conversations = conversations;
        _messages = messages;
        _blocks = blocks;
        _uow = uow;
        _logger = logger;
    }

    public async Task<Conversation> GetOrCreateConversationAsync(Guid viewerId, Guid hostId, CancellationToken ct = default)
    {
        // BR-DC-03: Check block in both directions
        if (await _blocks.ExistsAsync(viewerId, hostId, ct) || await _blocks.ExistsAsync(hostId, viewerId, ct))
            throw new DomainException("Cannot start conversation with a blocked user.");

        var existing = await _conversations.GetByParticipantsAsync(viewerId, hostId, ct);
        if (existing != null) return existing;

        var conversation = Conversation.Create(viewerId, hostId);
        await _conversations.AddAsync(conversation, ct);
        await _uow.SaveChangesAsync(ct);
        return conversation;
    }

    public Task<List<Conversation>> GetConversationsAsync(Guid userId, CancellationToken ct = default)
        => _conversations.GetVisibleByUserAsync(userId, ct);

    public Task<Conversation?> GetConversationByIdAsync(Guid conversationId, Guid userId, CancellationToken ct = default)
        => _conversations.GetByIdForUserAsync(conversationId, userId, ct);

    public async Task<List<DirectMessage>> GetMessagesAsync(
        Guid conversationId, Guid userId, DateTime from, DateTime? to = null, CancellationToken ct = default)
    {
        var conversation = await GetConversationOrThrowAsync(conversationId, userId, ct);
        // Partition safeguard: 'from' is required — enforced by interface signature
        return await _messages.GetMessagesAsync(conversationId, from, to, ct);
    }

    public async Task<DirectMessage> SendMessageAsync(
        Guid conversationId, Guid senderId, string content,
        MessageType messageType = MessageType.Text, string? emojiCode = null, CancellationToken ct = default)
    {
        var conversation = await GetConversationOrThrowAsync(conversationId, senderId, ct);

        // BR-DC-01: Check block
        var otherId = conversation.ViewerId == senderId ? conversation.HostId : conversation.ViewerId;
        if (await _blocks.ExistsAsync(senderId, otherId, ct) || await _blocks.ExistsAsync(otherId, senderId, ct))
            throw new DomainException("Cannot send message to a blocked user.");

        var message = DirectMessage.Create(conversationId, senderId, content, messageType, emojiCode);
        conversation.RecordMessage(content, senderId);

        await _messages.AddAsync(message, ct);
        await _conversations.UpdateAsync(conversation, ct);
        await _uow.SaveChangesAsync(ct);

        _logger.LogDebug("Message {MessageId} sent in conversation {ConversationId}", message.Id, conversationId);
        return message;
    }

    public async Task MarkAsReadAsync(Guid conversationId, Guid userId, CancellationToken ct = default)
    {
        var conversation = await GetConversationOrThrowAsync(conversationId, userId, ct);

        if (conversation.ViewerId == userId)
            conversation.MarkReadByViewer();
        else
            conversation.MarkReadByHost();

        await _conversations.UpdateAsync(conversation, ct);
        await _uow.SaveChangesAsync(ct);
    }

    public async Task BlockUserAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default)
    {
        if (await _blocks.ExistsAsync(blockerId, blockedId, ct))
            return; // Already blocked

        var block = Block.Create(blockerId, blockedId);
        await _blocks.AddAsync(block, ct);

        // BR-DC-03: Hide conversation from both parties
        var conversation = await _conversations.GetByParticipantsAsync(blockerId, blockedId, ct)
            ?? await _conversations.GetByParticipantsAsync(blockedId, blockerId, ct);

        if (conversation != null)
        {
            conversation.HideForBoth();
            await _conversations.UpdateAsync(conversation, ct);
        }

        await _uow.SaveChangesAsync(ct);
        _logger.LogInformation("User {BlockerId} blocked {BlockedId}", blockerId, blockedId);
    }

    public async Task UnblockUserAsync(Guid blockerId, Guid blockedId, CancellationToken ct = default)
    {
        await _blocks.DeleteAsync(blockerId, blockedId, ct);

        // Unhide conversation if no reverse block exists
        if (!await _blocks.ExistsAsync(blockedId, blockerId, ct))
        {
            var conversation = await _conversations.GetByParticipantsAsync(blockerId, blockedId, ct)
                ?? await _conversations.GetByParticipantsAsync(blockedId, blockerId, ct);

            if (conversation != null)
            {
                conversation.UnhideForBoth();
                await _conversations.UpdateAsync(conversation, ct);
            }
        }

        await _uow.SaveChangesAsync(ct);
    }

    public Task<bool> IsBlockedAsync(Guid userId1, Guid userId2, CancellationToken ct = default)
        => _blocks.ExistsInEitherDirectionAsync(userId1, userId2, ct);

    private async Task<Conversation> GetConversationOrThrowAsync(Guid conversationId, Guid userId, CancellationToken ct)
    {
        var conversation = await _conversations.GetByIdForUserAsync(conversationId, userId, ct);
        if (conversation == null) throw new NotFoundException("Conversation", conversationId);
        return conversation;
    }
}
