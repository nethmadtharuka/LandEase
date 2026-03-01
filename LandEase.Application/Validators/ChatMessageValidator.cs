using FluentValidation;
using LandEase.Application.DTOs.Ai;

namespace LandEase.Application.Validators;

public class ChatMessageValidator : AbstractValidator<ChatMessageDto>
{
    public ChatMessageValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message cannot be empty.")
            .MaximumLength(2000).WithMessage("Message must be under 2000 characters.");
    }
}