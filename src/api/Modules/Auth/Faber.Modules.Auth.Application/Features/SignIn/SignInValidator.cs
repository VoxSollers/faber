using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Auth.Application.Features.SignIn;

public class SignInValidator : Validator<SignInRequest>
{
    public SignInValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");
    }
}