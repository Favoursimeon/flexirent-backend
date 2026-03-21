using FlexiRent.Application.DTOs;
using FluentValidation;

namespace FlexiRent.Application.Validators;

public class UpdateProfileValidator : AbstractValidator<UpdateProfileDto>
{
    public UpdateProfileValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .MaximumLength(20)
            .Matches(@"^\+?[0-9\s\-()]+$").WithMessage("Invalid phone number format.")
            .When(x => x.Phone is not null);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.AddYears(-18))
            .WithMessage("You must be at least 18 years old.")
            .When(x => x.DateOfBirth.HasValue);
    }
}

public class EmergencyContactValidator : AbstractValidator<EmergencyContactDto>
{
    public EmergencyContactValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Contact name is required.")
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Contact phone is required.")
            .Matches(@"^\+?[0-9\s\-()]+$").WithMessage("Invalid phone number format.");

        RuleFor(x => x.Relation)
            .NotEmpty().WithMessage("Relation is required.")
            .MaximumLength(50);
    }
}