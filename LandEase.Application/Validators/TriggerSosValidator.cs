using FluentValidation;
using LandEase.Application.DTOs.Sos;

namespace LandEase.Application.Validators;

public class TriggerSosValidator : AbstractValidator<TriggerSosDto>
{
    public TriggerSosValidator()
    {
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Please describe your emergency.")
            .MaximumLength(1000).WithMessage("Description must be under 1000 characters.");

        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90)
            .WithMessage("Latitude must be between -90 and 90.");

        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180)
            .WithMessage("Longitude must be between -180 and 180.");
    }
}