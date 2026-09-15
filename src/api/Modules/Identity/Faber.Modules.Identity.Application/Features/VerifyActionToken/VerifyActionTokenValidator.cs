using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Identity.Application.Features.VerifyActionToken;

public class VerifyActionTokenValidator : Validator<VerifyActionTokenRequest>
{
    public VerifyActionTokenValidator()
    {
        RuleFor(x => x.CombinedKey.Value)
            .NotEmpty().WithMessage("Key value is required.")
            .Length(27, 200).WithMessage("Invalid key format.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid action");
    }
}