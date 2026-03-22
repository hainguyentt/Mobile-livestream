using LivestreamApp.Profiles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LivestreamApp.API.Controllers.V1;

[ApiController]
[Route("api/v1/host")]
[Authorize]
public class HostVerificationController : ControllerBase
{
    private readonly IHostVerificationService _hostVerificationService;

    public HostVerificationController(IHostVerificationService hostVerificationService) =>
        _hostVerificationService = hostVerificationService;

    /// <summary>Submits a host verification request. Requires Host role.</summary>
    [HttpPost("verification/request")]
    [Authorize(Roles = "Host")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RequestVerification(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var hostProfile = await _hostVerificationService.RequestVerificationAsync(userId, ct);
        return Ok(new { message = "Verification request submitted.", status = hostProfile.VerificationStatus });
    }

    /// <summary>Approves a host verification request. Admin only.</summary>
    [HttpPost("verification/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ApproveVerification([FromBody] VerificationActionRequest request, CancellationToken ct)
    {
        var adminId = GetCurrentUserId();
        var hostProfile = await _hostVerificationService.ApproveVerificationAsync(request.UserId, adminId, ct);
        return Ok(new { message = "Verification approved.", status = hostProfile.VerificationStatus });
    }

    /// <summary>Rejects a host verification request. Admin only.</summary>
    [HttpPost("verification/reject")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> RejectVerification([FromBody] VerificationRejectRequest request, CancellationToken ct)
    {
        var adminId = GetCurrentUserId();
        var hostProfile = await _hostVerificationService.RejectVerificationAsync(request.UserId, adminId, request.Note, ct);
        return Ok(new { message = "Verification rejected.", status = hostProfile.VerificationStatus });
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}

public record VerificationActionRequest(Guid UserId);
public record VerificationRejectRequest(Guid UserId, string? Note);
