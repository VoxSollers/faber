namespace Faber.Modules.Resumes.Application.Features.Skills.ReorderSkills;

public record ReorderSkillsRequest(
    Guid ResumeId,
    IReadOnlyList<Guid> OrderedIds);
