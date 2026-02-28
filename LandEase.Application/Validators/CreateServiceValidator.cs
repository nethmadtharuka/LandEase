using FluentValidation;
using LandEase.Application.DTOs.Services;

namespace LandEase.Application.Validators;

public class CreateServiceValidator : AbstractValidator<CreateServiceDto>
{
    public CreateServiceValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be under 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must be under 2000 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0).WithMessage("Price cannot be negative.");

        RuleFor(x => x.DestinationCountry)
            .NotEmpty().WithMessage("Destination country is required.");
    }
}