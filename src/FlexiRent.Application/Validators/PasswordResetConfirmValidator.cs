// PasswordResetConfirmValidator.cs
using FlexiRent.Application.DTOs;
using FluentValidation;

namespace FlexiRent.Application.Validators;

public class PasswordResetConfirmValidator : AbstractValidator<PasswordResetConfirmDto>
{
    public PasswordResetConfirmValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Reset token is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Please confirm your password.")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match.");
    }
}