using FluentValidation;
using LandEase.Application.DTOs.Bookings;

namespace LandEase.Application.Validators;

public class CreateBookingValidator : AbstractValidator<CreateBookingDto>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.ServiceId)
            .GreaterThan(0).WithMessage("A valid service must be selected.");

        RuleFor(x => x.ScheduledDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.ScheduledDate.HasValue)
            .WithMessage("Scheduled date must be in the future.");
    }
}