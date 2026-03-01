using FluentValidation;
using LandEase.Application.DTOs.Community;

namespace LandEase.Application.Validators;

public class CreatePostValidator : AbstractValidator<CreatePostDto>
{
    public CreatePostValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Post content cannot be empty.")
            .MaximumLength(2000).WithMessage("Post must be under 2000 characters.");
    }
}