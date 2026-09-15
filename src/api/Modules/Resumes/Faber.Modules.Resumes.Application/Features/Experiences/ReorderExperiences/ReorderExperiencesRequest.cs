namespace Faber.Modules.Resumes.Application.Features.Experiences.ReorderExperiences;

public record ReorderExperiencesRequest(
    Guid ResumeId,
    IReadOnlyList<Guid> OrderedIds);
