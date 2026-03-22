using FluentValidation;

namespace LivestreamApp.API.DTOs.Auth;

public record SendOtpRequest(string Target, string Purpose);

public class SendOtpRequestValidator : AbstractValidator<SendOtpRequest>
{
    public SendOtpRequestValidator()
    {
        RuleFor(x => x.Target).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Purpose).NotEmpty().Must(p => p is "EmailVerification" or "PhoneVerification" or "PasswordReset")
            .WithMessage("Purpose must be EmailVerification, PhoneVerification, or PasswordReset.");
    }
}
