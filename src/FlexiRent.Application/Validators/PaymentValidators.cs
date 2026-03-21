using FlexiRent.Application.DTOs;
using FluentValidation;

namespace FlexiRent.Application.Validators;

public class InitiatePaymentValidator : AbstractValidator<InitiatePaymentDto>
{
    public InitiatePaymentValidator()
    {
        RuleFor(x => x.LeaseId)
            .NotEmpty().WithMessage("Lease is required.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero.");
    }
}

public class CreateLeaseValidator : AbstractValidator<CreateLeaseDto>
{
    public CreateLeaseValidator()
    {
        RuleFor(x => x.PropertyId)
            .NotEmpty().WithMessage("Property is required.");

        RuleFor(x => x.MonthlyAmount)
            .GreaterThan(0).WithMessage("Monthly amount must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required.");

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.");

        RuleFor(x => x.PaymentDayOfMonth)
            .InclusiveBetween(1, 28)
            .WithMessage("Payment day must be between 1 and 28.");
    }
}

public class CreatePaymentAccountValidator : AbstractValidator<CreatePaymentAccountDto>
{
    public CreatePaymentAccountValidator()
    {
        RuleFor(x => x.AccountName)
            .NotEmpty().WithMessage("Account name is required.")
            .MaximumLength(200);

        RuleFor(x => x.AccountNumber)
            .NotEmpty().WithMessage("Account number is required.")
            .MaximumLength(50);
    }
}