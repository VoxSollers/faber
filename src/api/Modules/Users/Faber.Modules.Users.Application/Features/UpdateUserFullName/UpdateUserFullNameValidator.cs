using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Users.Application.Features.UpdateUserFullName;

public class UpdateUserFullNameValidator : Validator<UpdateUserFullNameRequest>
{
    public UpdateUserFullNameValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("FirstName is required");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("LastName is required");
    }
}