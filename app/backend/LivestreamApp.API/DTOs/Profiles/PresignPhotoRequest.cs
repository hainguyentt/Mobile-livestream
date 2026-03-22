using FluentValidation;

namespace LivestreamApp.API.DTOs.Profiles;

public record PresignPhotoRequest(int DisplayIndex, string ContentType, long FileSizeBytes);

public class PresignPhotoRequestValidator : AbstractValidator<PresignPhotoRequest>
{
    private static readonly string[] AllowedTypes = ["image/jpeg", "image/png", "image/webp"];

    public PresignPhotoRequestValidator()
    {
        RuleFor(x => x.DisplayIndex).InclusiveBetween(0, 5);
        RuleFor(x => x.ContentType).NotEmpty().Must(t => AllowedTypes.Contains(t))
            .WithMessage("Content type must be image/jpeg, image/png, or image/webp.");
        RuleFor(x => x.FileSizeBytes).GreaterThan(0).LessThanOrEqualTo(10_000_000)
            .WithMessage("File size must be between 1 byte and 10MB.");
    }
}
