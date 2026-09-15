using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Auth.Application.Features.ForgotPassword;

public class ForgotPasswordValidator : Validator<ForgotPasswordRequest>
{
    public ForgotPasswordValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }
}