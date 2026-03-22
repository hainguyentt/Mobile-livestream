using FluentValidation;

namespace LivestreamApp.API.DTOs.Auth;

public record VerifyOtpRequest(string Target, string Code, string Purpose);

public class VerifyOtpRequestValidator : AbstractValidator<VerifyOtpRequest>
{
    public VerifyOtpRequestValidator()
    {
        RuleFor(x => x.Target).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Code).NotEmpty().Length(6).Matches("^[0-9]{6}$").WithMessage("OTP must be 6 digits.");
        RuleFor(x => x.Purpose).NotEmpty();
    }
}
