namespace Faber.Modules.Resumes.Application.Features.Skills.GetSkill;

public static class GetSkillMapper
{
    public static GetSkillCommand MapToCommand(this GetSkillRequest request)
    {
        return new GetSkillCommand(request.ResumeId, request.Id);
    }

    public static GetSkillResponse MapToResponse(this Domain.Entities.Skill skill)
    {
        return new GetSkillResponse(
            skill.Id,
            skill.ResumeId,
            skill.Name,
            skill.Level,
            skill.Order);
    }
}