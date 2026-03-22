using FluentValidation;

namespace LivestreamApp.API.DTOs.Profiles;

public record ReorderPhotosRequest(Guid[] OrderedPhotoIds);

public class ReorderPhotosRequestValidator : AbstractValidator<ReorderPhotosRequest>
{
    public ReorderPhotosRequestValidator()
    {
        RuleFor(x => x.OrderedPhotoIds).NotNull().NotEmpty()
            .Must(ids => ids.Length <= 6).WithMessage("Maximum 6 photos allowed.")
            .Must(ids => ids.Distinct().Count() == ids.Length).WithMessage("Duplicate photo IDs are not allowed.");
    }
}
