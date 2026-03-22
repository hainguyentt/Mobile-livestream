using FluentValidation;

namespace LivestreamApp.API.DTOs.Auth;

public record ResetPasswordRequest(string Email, string NewPassword, string OtpCode);

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(100)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one digit.");
        RuleFor(x => x.OtpCode).NotEmpty().Length(6).Matches("^[0-9]{6}$");
    }
}
