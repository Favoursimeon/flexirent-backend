using FlexiRent.Application.DTOs;
using FluentValidation;

namespace FlexiRent.Application.Validators;

public class CreatePropertyValidator : AbstractValidator<CreatePropertyDto>
{
    public CreatePropertyValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(5000);

        RuleFor(x => x.PricePerMonth)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.Region)
            .NotEmpty().WithMessage("Region is required.")
            .MaximumLength(100);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(500);

        RuleFor(x => x.Bedrooms)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.Bathrooms)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.AreaSqft)
            .GreaterThan(0).WithMessage("Area must be greater than zero.");
    }
}

public class UpdatePropertyValidator : AbstractValidator<UpdatePropertyDto>
{
    public UpdatePropertyValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(5000);

        RuleFor(x => x.PricePerMonth)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");

        RuleFor(x => x.Region)
            .NotEmpty().WithMessage("Region is required.")
            .MaximumLength(100);

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("Address is required.")
            .MaximumLength(500);
    }
}