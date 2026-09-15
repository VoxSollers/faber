namespace Faber.Modules.Resumes.Application.Features.Skills.UpdateSkill;

public record UpdateSkillRequest(
    Guid Id,
    Guid ResumeId,
    string? Name,
    string? Level);