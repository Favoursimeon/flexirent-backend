using FlexiRent.Application.DTOs;
using FluentValidation;

namespace FlexiRent.Application.Validators;

public class CreateBookingValidator : AbstractValidator<CreateBookingDto>
{
    public CreateBookingValidator()
    {
        RuleFor(x => x.ProviderId)
            .NotEmpty().WithMessage("Provider is required.");

        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property is required.");

        RuleFor(x => x.StartAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Start time must be in the future.");

        RuleFor(x => x.EndAt)
            .GreaterThan(x => x.StartAt).WithMessage("End time must be after start time.");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).When(x => x.Notes is not null);
    }
}

public class CreateViewingValidator : AbstractValidator<CreateViewingDto>
{
    public CreateViewingValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property is required.");

        RuleFor(x => x.ScheduledAt)
            .GreaterThan(DateTime.UtcNow).WithMessage("Viewing must be scheduled in the future.");
    }
}

public class CancelBookingValidator : AbstractValidator<CancelBookingDto>
{
    public CancelBookingValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Cancellation reason is required.")
            .MaximumLength(500);
    }
}