using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Users.Application.Features.GetUserById;

public class GetUserByIdValidator : Validator<GetUserByIdRequest>
{
    public GetUserByIdValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}