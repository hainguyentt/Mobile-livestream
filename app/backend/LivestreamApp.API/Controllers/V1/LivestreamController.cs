using LivestreamApp.API.DTOs.Livestream;
using LivestreamApp.Livestream.Services;
using LivestreamApp.Shared.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LivestreamApp.API.Controllers.V1;

[ApiController]
[Route("api/v1/livestream")]
[Authorize]
public sealed class LivestreamController : ControllerBase
{
    private readonly ILivestreamService _livestream;

    public LivestreamController(ILivestreamService livestream)
    {
        _livestream = livestream;
    }

    /// <summary>POST /api/v1/livestream/rooms — Host creates a new room.</summary>
    [HttpPost("rooms")]
    public async Task<IActionResult> CreateRoom([FromBody] CreateRoomRequest request, CancellationToken ct)
    {
        var hostId = GetUserId();
        var room = await _livestream.CreateRoomAsync(hostId, request.Title, request.Category, ct);
        return CreatedAtAction(nameof(GetRoom), new { id = room.Id }, RoomResponse.From(room));
    }

    /// <summary>GET /api/v1/livestream/rooms — List active rooms with optional filter.</summary>
    [HttpGet("rooms")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRooms([FromQuery] GetRoomsQuery query, CancellationToken ct)
    {
        var (rooms, total) = await _livestream.GetActiveRoomsAsync(query.Category, query.Page, query.PageSize, ct);
        return Ok(new
        {
            items = rooms.Select(RoomResponse.From),
            total,
            page = query.Page,
            pageSize = query.PageSize
        });
    }

    /// <summary>GET /api/v1/livestream/rooms/{id} — Get room details.</summary>
    [HttpGet("rooms/{id:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRoom(Guid id, CancellationToken ct)
    {
        var room = await _livestream.GetRoomByIdAsync(id, ct);
        if (room == null) return NotFound();
        return Ok(RoomResponse.From(room));
    }

    /// <summary>POST /api/v1/livestream/rooms/{id}/start — Host starts the stream.</summary>
    [HttpPost("rooms/{id:guid}/start")]
    public async Task<IActionResult> StartStream(Guid id, CancellationToken ct)
    {
        await _livestream.StartStreamAsync(id, GetUserId(), ct);
        return NoContent();
    }

    /// <summary>POST /api/v1/livestream/rooms/{id}/end — Host ends the stream.</summary>
    [HttpPost("rooms/{id:guid}/end")]
    public async Task<IActionResult> EndStream(Guid id, CancellationToken ct)
    {
        await _livestream.EndStreamAsync(id, GetUserId(), ct);
        return NoContent();
    }

    /// <summary>POST /api/v1/livestream/rooms/{id}/join — Viewer joins a room.</summary>
    [HttpPost("rooms/{id:guid}/join")]
    public async Task<IActionResult> JoinRoom(Guid id, CancellationToken ct)
    {
        var (room, session) = await _livestream.JoinRoomAsync(id, GetUserId(), ct);
        return Ok(new { roomId = room.Id, sessionId = session.Id, agoraChannelName = room.AgoraChannelName });
    }

    /// <summary>POST /api/v1/livestream/rooms/{id}/leave — Viewer leaves a room.</summary>
    [HttpPost("rooms/{id:guid}/leave")]
    public async Task<IActionResult> LeaveRoom(Guid id, CancellationToken ct)
    {
        await _livestream.LeaveRoomAsync(id, GetUserId(), ct);
        return NoContent();
    }

    /// <summary>POST /api/v1/livestream/rooms/{id}/ban/{userId} — Host bans a viewer.</summary>
    [HttpPost("rooms/{id:guid}/ban/{userId:guid}")]
    public async Task<IActionResult> BanViewer(Guid id, Guid userId, [FromBody] BanViewerRequest? request, CancellationToken ct)
    {
        await _livestream.BanViewerAsync(id, GetUserId(), userId, request?.Reason, ct);
        return NoContent();
    }

    /// <summary>GET /api/v1/livestream/rooms/{id}/viewers — Get active viewer count.</summary>
    [HttpGet("rooms/{id:guid}/viewers")]
    public async Task<IActionResult> GetViewers(Guid id, CancellationToken ct)
    {
        var count = await _livestream.GetViewerCountAsync(id, ct);
        return Ok(new { roomId = id, viewerCount = count });
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
