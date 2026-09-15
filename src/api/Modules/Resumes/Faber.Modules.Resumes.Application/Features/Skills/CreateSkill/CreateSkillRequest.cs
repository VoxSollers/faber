namespace Faber.Modules.Resumes.Application.Features.Skills.CreateSkill;

public record CreateSkillRequest(
    Guid ResumeId,
    string? Name,
    string? Level);