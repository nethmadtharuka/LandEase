using FluentValidation;
using LandEase.Application.DTOs.Kyc;

namespace LandEase.Application.Validators;

public class KycSubmissionValidator : AbstractValidator<KycSubmissionDto>
{
    private readonly string[] _allowedTypes = { "image/jpeg", "image/png", "application/pdf" };
    private const long MaxFileSize = 5 * 1024 * 1024;

    public KycSubmissionValidator()
    {
        RuleFor(x => x.IdDocument)
            .NotNull().WithMessage("ID document is required.")
            .Must(f => f.Length <= MaxFileSize)
            .WithMessage("ID document must be under 5MB.")
            .Must(f => _allowedTypes.Contains(f.ContentType))
            .WithMessage("ID document must be JPG, PNG, or PDF.");

        RuleFor(x => x.Selfie)
            .NotNull().WithMessage("Selfie is required.")
            .Must(f => f.Length <= MaxFileSize)
            .WithMessage("Selfie must be under 5MB.")
            .Must(f => _allowedTypes.Contains(f.ContentType))
            .WithMessage("Selfie must be JPG or PNG.");
    }
}
