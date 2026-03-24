using FluentValidation;
using LivestreamApp.Shared.Domain.Enums;

namespace LivestreamApp.API.DTOs.Livestream;

public sealed record CreateRoomRequest(string Title, RoomCategory Category);

public sealed class CreateRoomRequestValidator : AbstractValidator<CreateRoomRequest>
{
    public CreateRoomRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(100).WithMessage("Title must not exceed 100 characters.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid room category.");
    }
}
