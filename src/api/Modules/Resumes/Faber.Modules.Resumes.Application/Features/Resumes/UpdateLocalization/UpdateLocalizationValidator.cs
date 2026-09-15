using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateLocalization;

public class UpdateLocalizationValidator : Validator<UpdateLocalizationRequest>
{
    public UpdateLocalizationValidator()
    {
        RuleFor(x => x.Localization)
            .NotEmpty().WithMessage("'Localization' must not be empty.")
            .MaximumLength(100).WithMessage("'Localization' must not exceed 100 characters.");
    }
}
