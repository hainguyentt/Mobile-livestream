using FluentValidation;

namespace LivestreamApp.API.DTOs.Profiles;

public record ConfirmPhotoRequest(Guid PhotoId, int DisplayIndex, string S3Key, string S3Url, long FileSizeBytes, string MimeType);

public class ConfirmPhotoRequestValidator : AbstractValidator<ConfirmPhotoRequest>
{
    public ConfirmPhotoRequestValidator()
    {
        RuleFor(x => x.PhotoId).NotEmpty();
        RuleFor(x => x.DisplayIndex).InclusiveBetween(0, 5);
        RuleFor(x => x.S3Key).NotEmpty().MaximumLength(500);
        RuleFor(x => x.S3Url).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.FileSizeBytes).GreaterThan(0);
        RuleFor(x => x.MimeType).NotEmpty();
    }
}
