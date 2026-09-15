namespace Faber.Modules.Resumes.Application.Features.Skills.CreateSkill;

public static class CreateSkillMapper
{
    public static CreateSkillCommand MapToCommand(this CreateSkillRequest request)
    {
        return new CreateSkillCommand(
            request.ResumeId,
            request.Name,
            request.Level);
    }

    public static CreateSkillResponse MapToResponse(this Domain.Entities.Skill skill)
    {
        return new CreateSkillResponse(
            skill.Id,
            skill.ResumeId,
            skill.Name,
            skill.Level,
            skill.Order);
    }
}