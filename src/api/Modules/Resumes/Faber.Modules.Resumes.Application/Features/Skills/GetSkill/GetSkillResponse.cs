namespace Faber.Modules.Resumes.Application.Features.Skills.GetSkill;

public record GetSkillResponse(
    Guid Id,
    Guid ResumeId,
    string? Name,
    string? Level,
    int Order);