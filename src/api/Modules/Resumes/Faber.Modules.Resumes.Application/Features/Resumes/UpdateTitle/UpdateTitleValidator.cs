using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateTitle;

public class UpdateTitleValidator : Validator<UpdateTitleRequest>
{
    public UpdateTitleValidator()
    {
        RuleFor(x => x.Title)
            .Must(value => (value ?? string.Empty).Trim().Length <= 100)
            .WithMessage("'Title' must not exceed 100 characters.");
    }
}
