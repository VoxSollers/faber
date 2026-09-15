using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Auth.Application.Features.VerifyEmail;

public class VerifyEmailValidator : Validator<VerifyEmailRequest>
{
    public VerifyEmailValidator()
    {
        RuleFor(x => x.CombinedKey.Value)
            .NotEmpty().WithMessage("Key value is required")
            .Length(27, 200).WithMessage("Invalid key format");
    }
}