using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Projects.ReorderProjects;

public record ReorderProjectsCommand(
    Guid ResumeId,
    IReadOnlyList<Guid> OrderedIds) : ICommand<ErrorOr<bool>>;
