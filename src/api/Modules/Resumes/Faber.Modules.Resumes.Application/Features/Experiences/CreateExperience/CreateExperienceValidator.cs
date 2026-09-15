using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Resumes.Application.Features.Experiences.CreateExperience;

public class CreateExperienceValidator : Validator<CreateExperienceRequest>
{
    public CreateExperienceValidator()
    {
        RuleFor(x => x.JobTitle)
            .MaximumLength(100).WithMessage("'JobTitle' must not exceed 100 characters.");

        RuleFor(x => x.Employer)
            .MaximumLength(100).WithMessage("'Employer' must not exceed 100 characters.");

        RuleFor(x => x.City)
            .MaximumLength(100).WithMessage("'City' must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("'Description' must not exceed 1000 characters.");

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today))
            .WithMessage("'StartDate' must not be in the future.")
            .When(x => x.StartDate.HasValue);

        RuleFor(x => x.EndDate)
            .Must((r, endDate) => endDate >= r.StartDate)
            .WithMessage("'EndDate' must be greater than or equal to 'StartDate'.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
