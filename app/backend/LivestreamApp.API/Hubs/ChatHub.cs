using LivestreamApp.DirectChat.Services;
using LivestreamApp.RoomChat.Services;
using LivestreamApp.Shared.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LivestreamApp.API.Hubs;

/// <summary>
/// SignalR hub for chat events: room chat, direct messages, call signaling, billing notifications.
/// Requires JWT authentication.
/// </summary>
[Authorize]
public sealed class ChatHub : Hub
{
    private readonly IRoomChatService _roomChat;
    private readonly IDirectChatService _directChat;
    private readonly IConnectionTracker _tracker;

    public ChatHub(IRoomChatService roomChat, IDirectChatService directChat, IConnectionTracker tracker)
    {
        _roomChat = roomChat;
        _directChat = directChat;
        _tracker = tracker;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        _tracker.Add(Context.ConnectionId, userId);

        // Join personal group for direct messages and call signaling
        await Groups.AddToGroupAsync(Context.ConnectionId, DirectGroup(userId));
        await base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception? exception)
    {
        _tracker.Remove(Context.ConnectionId);
        return base.OnDisconnectedAsync(exception);
    }

    /// <summary>Sends a message to a room chat stream and broadcasts to room group.</summary>
    public async Task SendRoomMessage(Guid roomId, string text)
    {
        var userId = GetUserId();
        var displayName = GetDisplayName();

        var message = await _roomChat.SendMessageAsync(roomId, userId, displayName, text);

        await Clients.Group(RoomGroup(roomId)).SendAsync("RoomMessageReceived", new
        {
            messageId = message.MessageId,
            roomId,
            senderId = userId,
            senderDisplayName = displayName,
            content = message.Content,
            sentAt = message.SentAt
        });
    }

    /// <summary>Sends a direct message and delivers to recipient if online.</summary>
    public async Task SendDirectMessage(Guid conversationId, string text)
    {
        var userId = GetUserId();
        var message = await _directChat.SendMessageAsync(conversationId, userId, text);

        // Get the other participant's ID from the conversation
        var conversation = await _directChat.GetConversationByIdAsync(conversationId, userId);
        if (conversation == null) return;

        var recipientId = conversation.ViewerId == userId ? conversation.HostId : conversation.ViewerId;

        await Clients.Group(DirectGroup(recipientId)).SendAsync("DirectMessageReceived", new
        {
            messageId = message.Id,
            conversationId,
            senderId = userId,
            content = message.Content,
            messageType = message.MessageType.ToString(),
            sentAt = message.SentAt
        });
    }

    /// <summary>Marks a conversation as read by the current user.</summary>
    public async Task MarkConversationRead(Guid conversationId)
    {
        var userId = GetUserId();
        await _directChat.MarkAsReadAsync(conversationId, userId);
    }

    // Server → Client events (for documentation):
    // RoomMessageReceived, DirectMessageReceived
    // CallRequest, CallAccepted, CallRejected, CallEnded
    // BalanceUpdated, LowBalanceWarning
    // ConversationHidden

    private Guid GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("Unauthorized");
        return Guid.Parse(claim);
    }

    private string GetDisplayName()
        => Context.User?.FindFirst("display_name")?.Value ?? "User";

    private static string RoomGroup(Guid roomId) => $"room:{roomId}";
    private static string DirectGroup(Guid userId) => $"direct:{userId}";
}
