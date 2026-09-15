namespace Faber.Modules.Resumes.Application.Features.Skills.CreateSkill;

public record CreateSkillResponse(
    Guid Id,
    Guid ResumeId,
    string? Name,
    string? Level,
    int Order);