using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Users.Application.Features.GetUserByEmail;

public class GetUserByEmailValidator : Validator<GetUserByEmailRequest>
{
    public GetUserByEmailValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("selector is required.")
            .EmailAddress().WithMessage("Invalid email format.");
    }
}