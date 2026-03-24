using LivestreamApp.Livestream.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace LivestreamApp.API.Hubs;

/// <summary>
/// SignalR hub for livestream room events: viewer join/leave, viewer count, gifts, bans.
/// Requires JWT authentication.
/// </summary>
[Authorize]
public sealed class LivestreamHub : Hub
{
    private readonly ILivestreamService _livestream;
    private readonly IConnectionTracker _tracker;

    public LivestreamHub(ILivestreamService livestream, IConnectionTracker tracker)
    {
        _livestream = livestream;
        _tracker = tracker;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var roomId = GetRoomIdFromQuery();

        if (roomId.HasValue)
        {
            _tracker.Add(Context.ConnectionId, userId, roomId);
            await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(roomId.Value));
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));

            var count = await _livestream.GetViewerCountAsync(roomId.Value);
            await Clients.Group(RoomGroup(roomId.Value))
                .SendAsync("ViewerJoined", new { userId, viewerCount = count });
        }
        else
        {
            _tracker.Add(Context.ConnectionId, userId);
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetUserId();
        var roomId = _tracker.GetRoomId(Context.ConnectionId);

        _tracker.Remove(Context.ConnectionId);

        if (roomId.HasValue)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, RoomGroup(roomId.Value));
            await _livestream.LeaveRoomAsync(roomId.Value, userId);

            var count = await _livestream.GetViewerCountAsync(roomId.Value);
            await Clients.Group(RoomGroup(roomId.Value))
                .SendAsync("ViewerLeft", new { userId, viewerCount = count });
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>Client calls this to explicitly join a room group.</summary>
    public async Task JoinRoom(Guid roomId)
    {
        var userId = GetUserId();
        await Groups.AddToGroupAsync(Context.ConnectionId, RoomGroup(roomId));
        _tracker.Add(Context.ConnectionId, userId, roomId);
    }

    /// <summary>Client calls this to explicitly leave a room group.</summary>
    public async Task LeaveRoom(Guid roomId)
    {
        var userId = GetUserId();
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, RoomGroup(roomId));
        await _livestream.LeaveRoomAsync(roomId, userId);
    }

    /// <summary>Viewer sends a gift to the host.</summary>
    public async Task SendGift(Guid roomId, string giftId)
    {
        var userId = GetUserId();
        // Gift processing is handled by REST API — hub just broadcasts
        await Clients.Group(RoomGroup(roomId))
            .SendAsync("GiftReceived", new { senderId = userId, giftId, timestamp = DateTime.UtcNow });
    }

    // Server → Client event names (for documentation)
    // ViewerJoined, ViewerLeft, ViewerCountUpdated, StreamEnded, ViewerBanned, GiftReceived

    private Guid GetUserId()
    {
        var claim = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new HubException("Unauthorized");
        return Guid.Parse(claim);
    }

    private Guid? GetRoomIdFromQuery()
    {
        var roomIdStr = Context.GetHttpContext()?.Request.Query["roomId"].ToString();
        return Guid.TryParse(roomIdStr, out var roomId) ? roomId : null;
    }

    private static string RoomGroup(Guid roomId) => $"room:{roomId}";
    private static string UserGroup(Guid userId) => $"direct:{userId}";
}
