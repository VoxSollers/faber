using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Resumes.Application.Features.Courses.UpdateCourse;

public class UpdateCourseValidator : Validator<UpdateCourseRequest>
{
    public UpdateCourseValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("'Name' must not exceed 100 characters.");

        RuleFor(x => x.School)
            .MaximumLength(100).WithMessage("'School' must not exceed 100 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("'Description' must not exceed 1000 characters.");

        RuleFor(x => x.EndDate)
            .Must((r, endDate) => endDate >= r.StartDate)
            .WithMessage("'EndDate' must be greater than or equal to 'StartDate'.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);
    }
}
