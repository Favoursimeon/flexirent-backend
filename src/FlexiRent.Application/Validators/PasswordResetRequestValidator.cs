// PasswordResetRequestValidator.cs
using FlexiRent.Application.DTOs;
using FluentValidation;

namespace FlexiRent.Application.Validators;

public class PasswordResetRequestValidator : AbstractValidator<PasswordResetRequestDto>
{
    public PasswordResetRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}