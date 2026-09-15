using FastEndpoints;
using FluentValidation;

namespace Faber.Modules.Resumes.Application.Features.Educations.ReorderEducations;

public class ReorderEducationsValidator : Validator<ReorderEducationsRequest>
{
    public ReorderEducationsValidator()
    {
        RuleFor(x => x.ResumeId)
            .NotEmpty().WithMessage("'ResumeId' must not be empty.");

        RuleFor(x => x.OrderedIds)
            .NotEmpty().WithMessage("'OrderedIds' must not be empty.")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("'OrderedIds' must not contain duplicates.")
            .When(x => x.OrderedIds is not null);
    }
}
