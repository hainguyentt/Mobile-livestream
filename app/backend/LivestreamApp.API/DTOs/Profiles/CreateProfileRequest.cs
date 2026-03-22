using System.ComponentModel.DataAnnotations;

namespace LivestreamApp.API.DTOs.Profiles;

public record CreateProfileRequest(
    [Required, StringLength(50, MinimumLength = 2)] string DisplayName,
    [Required] DateOnly DateOfBirth
);
