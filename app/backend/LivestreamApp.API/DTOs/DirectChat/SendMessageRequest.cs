using FluentValidation;
using LivestreamApp.Shared.Domain.Enums;

namespace LivestreamApp.API.DTOs.DirectChat;

public sealed record SendMessageRequest(string Content, MessageType MessageType = MessageType.Text, string? EmojiCode = null);

public sealed class SendMessageRequestValidator : AbstractValidator<SendMessageRequest>
{
    public SendMessageRequestValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Content is required.")
            .MaximumLength(1000).WithMessage("Content must not exceed 1000 characters.");

        RuleFor(x => x.EmojiCode)
            .NotEmpty().When(x => x.MessageType == MessageType.Emoji)
            .WithMessage("EmojiCode is required for emoji messages.");
    }
}
