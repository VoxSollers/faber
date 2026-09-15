namespace Faber.Modules.Resumes.Application.Features.Educations.ReorderEducations;

public record ReorderEducationsRequest(
    Guid ResumeId,
    IReadOnlyList<Guid> OrderedIds);
