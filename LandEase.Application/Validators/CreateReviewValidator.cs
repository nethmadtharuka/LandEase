using FluentValidation;
using LandEase.Application.DTOs.Reviews;

namespace LandEase.Application.Validators;

public class CreateReviewValidator : AbstractValidator<CreateReviewDto>
{
    public CreateReviewValidator()
    {
        RuleFor(x => x.BookingId)
            .GreaterThan(0).WithMessage("A valid booking is required.");

        RuleFor(x => x.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(x => x.Comment)
            .MaximumLength(1000)
            .When(x => x.Comment != null)
            .WithMessage("Comment must be under 1000 characters.");
    }
}