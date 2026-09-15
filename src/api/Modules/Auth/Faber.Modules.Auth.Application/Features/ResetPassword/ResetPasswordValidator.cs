using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Auth.Application.Features.ResetPassword;

public class ResetPasswordValidator : Validator<ResetPasswordRequest>
{
    public ResetPasswordValidator()
    {
        RuleFor(x => x.CombinedKey.Value)
            .NotEmpty().WithMessage("Key value is required")
            .Length(27, 200).WithMessage("Invalid key format");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("New password is required")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
    }
}