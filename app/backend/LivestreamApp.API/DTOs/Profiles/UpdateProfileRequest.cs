using FluentValidation;

namespace LivestreamApp.API.DTOs.Profiles;

public record UpdateProfileRequest(string? Bio, string[]? Interests, string? PreferredLanguage);

public class UpdateProfileRequestValidator : AbstractValidator<UpdateProfileRequest>
{
    public UpdateProfileRequestValidator()
    {
        RuleFor(x => x.Bio).MaximumLength(500).When(x => x.Bio is not null);
        RuleFor(x => x.Interests).Must(i => i == null || i.Length <= 20)
            .WithMessage("Maximum 20 interests allowed.");
        RuleFor(x => x.PreferredLanguage).Length(2, 5).When(x => x.PreferredLanguage is not null);
    }
}
