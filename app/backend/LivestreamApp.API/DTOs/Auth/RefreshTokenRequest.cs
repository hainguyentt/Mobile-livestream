using FluentValidation;

namespace LivestreamApp.API.DTOs.Auth;

public record RefreshTokenRequest(string RefreshToken);

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
