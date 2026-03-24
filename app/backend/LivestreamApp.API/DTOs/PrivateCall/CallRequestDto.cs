using FluentValidation;

namespace LivestreamApp.API.DTOs.PrivateCall;

public sealed record CallRequestDto(Guid HostId);

public sealed record RejectCallRequest(string? Reason);

public sealed class CallRequestDtoValidator : AbstractValidator<CallRequestDto>
{
    public CallRequestDtoValidator()
    {
        RuleFor(x => x.HostId)
            .NotEmpty().WithMessage("HostId is required.");
    }
}
