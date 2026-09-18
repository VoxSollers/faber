namespace Faber.Modules.Resumes.Application.Features.Projects.ReorderProjects;

public record ReorderProjectsRequest(
    Guid ResumeId,
    IReadOnlyList<Guid> OrderedIds);
