using Faber.Modules.Identity.PublicApi;
using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Auth.Application.Features.SignUp;

public class SignUpValidator : Validator<SignUpRequest>
{
    public SignUpValidator(IIdentityModuleApi identityModuleApi)
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required")
            .MustAsync(async (username, cancellationToken) =>
                await identityModuleApi.IsUsernameUniqueAsync(username, cancellationToken))
            .WithMessage("Username already exists");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email")
            .MustAsync(async (email, cancellationToken) =>
                await identityModuleApi.IsEmailUniqueAsync(email, cancellationToken))
            .WithMessage("Email already exists");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required")
            .Equal(x => x.Password).WithMessage("Passwords do not match");
    }
}