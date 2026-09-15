using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Experiences.ReorderExperiences;

public record ReorderExperiencesCommand(
    Guid ResumeId,
    IReadOnlyList<Guid> OrderedIds) : ICommand<ErrorOr<bool>>;
