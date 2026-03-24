using LivestreamApp.API.DTOs.PrivateCall;
using LivestreamApp.Livestream.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LivestreamApp.API.Controllers.V1;

[ApiController]
[Route("api/v1/livestream/calls")]
[Authorize]
public sealed class PrivateCallController : ControllerBase
{
    private readonly IPrivateCallService _calls;

    public PrivateCallController(IPrivateCallService calls)
    {
        _calls = calls;
    }

    /// <summary>POST /api/v1/livestream/calls/request — Viewer requests a private call.</summary>
    [HttpPost("request")]
    public async Task<IActionResult> RequestCall([FromBody] CallRequestDto request, CancellationToken ct)
    {
        var viewerId = GetUserId();
        var callRequest = await _calls.RequestCallAsync(viewerId, request.HostId, ct);
        return CreatedAtAction(nameof(GetCallStatus), new { id = callRequest.Id }, new
        {
            requestId = callRequest.Id,
            status = callRequest.Status.ToString(),
            expiresAt = callRequest.ExpiresAt
        });
    }

    /// <summary>POST /api/v1/livestream/calls/{id}/accept — Host accepts a call request.</summary>
    [HttpPost("{id:guid}/accept")]
    public async Task<IActionResult> AcceptCall(Guid id, CancellationToken ct)
    {
        var hostId = GetUserId();
        var (session, hostToken, viewerToken) = await _calls.AcceptCallAsync(id, hostId, ct);
        return Ok(new
        {
            sessionId = session.Id,
            agoraChannelName = session.AgoraChannelName,
            hostToken = new { token = hostToken.Token, expiresAt = hostToken.ExpiresAt },
            viewerToken = new { token = viewerToken.Token, expiresAt = viewerToken.ExpiresAt }
        });
    }

    /// <summary>POST /api/v1/livestream/calls/{id}/reject — Host rejects a call request.</summary>
    [HttpPost("{id:guid}/reject")]
    public async Task<IActionResult> RejectCall(Guid id, [FromBody] RejectCallRequest? request, CancellationToken ct)
    {
        await _calls.RejectCallAsync(id, GetUserId(), request?.Reason, ct);
        return NoContent();
    }

    /// <summary>POST /api/v1/livestream/calls/{id}/end — Participant ends an active call.</summary>
    [HttpPost("{id:guid}/end")]
    public async Task<IActionResult> EndCall(Guid id, CancellationToken ct)
    {
        var userId = GetUserId();
        await _calls.EndCallAsync(id, userId, "User", ct);
        return NoContent();
    }

    /// <summary>GET /api/v1/livestream/calls/{id}/token — Get/refresh Agora token for active call.</summary>
    [HttpGet("{id:guid}/token")]
    public async Task<IActionResult> GetToken(Guid id, CancellationToken ct)
    {
        var token = await _calls.GetAgoraTokenAsync(id, GetUserId(), ct);
        return Ok(new { token = token.Token, channelName = token.ChannelName, expiresAt = token.ExpiresAt });
    }

    /// <summary>GET /api/v1/livestream/calls/{id}/status — Get call session status.</summary>
    [HttpGet("{id:guid}/status")]
    public async Task<IActionResult> GetCallStatus(Guid id, CancellationToken ct)
    {
        var session = await _calls.GetCallStatusAsync(id, ct);
        if (session == null) return NotFound();
        return Ok(new
        {
            sessionId = session.Id,
            status = session.Status.ToString(),
            startedAt = session.StartedAt,
            endedAt = session.EndedAt,
            totalCoinsCharged = session.TotalCoinsCharged,
            totalTicks = session.TotalTicks
        });
    }

    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
}
