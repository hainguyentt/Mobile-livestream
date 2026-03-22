using LivestreamApp.API.DTOs.Profiles;
using LivestreamApp.Profiles.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LivestreamApp.API.Controllers.V1;

[ApiController]
[Route("api/v1/profiles")]
[Authorize]
public class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IPhotoService _photoService;

    public ProfilesController(IProfileService profileService, IPhotoService photoService)
    {
        _profileService = profileService;
        _photoService = photoService;
    }

    /// <summary>Creates the current user's profile (first-time setup).</summary>
    [HttpPost("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProfile([FromBody] CreateProfileRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var profile = await _profileService.CreateProfileAsync(userId, request.DisplayName, request.DateOfBirth, ct);
        return Ok(profile);
    }

    /// <summary>Returns the current user's profile with photos.</summary>
    [HttpGet("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyProfile(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var profile = await _profileService.GetProfileAsync(userId, ct);
        return Ok(profile);
    }

    /// <summary>Updates bio, interests, and preferred language.</summary>
    [HttpPut("me")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var profile = await _profileService.UpdateProfileAsync(userId, request.Bio, request.Interests, request.PreferredLanguage, ct);
        return Ok(profile);
    }

    /// <summary>Generates a presigned S3 URL for direct photo upload.</summary>
    [HttpPost("photos/presign")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> PresignPhoto([FromBody] PresignPhotoRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var (uploadUrl, photoId) = await _photoService.GeneratePresignedUploadUrlAsync(
            userId, request.DisplayIndex, request.ContentType, request.FileSizeBytes, ct);

        // Return s3Key explicitly so the client does not need to parse it from the upload URL
        var s3Key = $"photos/{userId}/{photoId}";
        return Ok(new { uploadUrl, photoId, s3Key });
    }

    /// <summary>Confirms a photo upload after the client has PUT the file to S3.</summary>
    [HttpPost("photos/confirm")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> ConfirmPhoto([FromBody] ConfirmPhotoRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        var photo = await _photoService.ConfirmPhotoUploadAsync(
            userId, request.PhotoId, request.DisplayIndex, request.S3Key, request.S3Url, request.FileSizeBytes, request.MimeType, ct);

        return StatusCode(StatusCodes.Status201Created, photo);
    }

    /// <summary>Deletes a photo and removes it from S3.</summary>
    [HttpDelete("photos/{photoId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeletePhoto(Guid photoId, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _photoService.DeletePhotoAsync(userId, photoId, ct);
        return NoContent();
    }

    /// <summary>Reorders photos by assigning new display indices.</summary>
    [HttpPut("photos/reorder")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ReorderPhotos([FromBody] ReorderPhotosRequest request, CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        await _photoService.ReorderPhotosAsync(userId, request.OrderedPhotoIds, ct);
        return Ok(new { message = "Photos reordered successfully." });
    }

    private Guid GetCurrentUserId()
    {
        var claim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var id) ? id : Guid.Empty;
    }
}
