using FluentValidation;

namespace LivestreamApp.API.DTOs.Auth;

public record LoginRequest(string Email, string Password, string? CaptchaToken = null);

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
