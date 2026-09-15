using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Users.Application.Features.GetUserByName;

public class GetUserByNameValidator : Validator<GetUserByNameRequest>
{
    public GetUserByNameValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required");
    }
}