namespace Faber.Modules.Resumes.Application.Features.Skills.GetAllSkills;

public static class GetAllSkillsMapper
{
    public static GetAllSkillsCommand MapToCommand(this GetAllSkillsRequest request)
    {
        return new GetAllSkillsCommand(request.ResumeId);
    }

    public static GetAllSkillsItem MapToItem(this Domain.Entities.Skill skill)
    {
        return new GetAllSkillsItem(
            skill.Id,
            skill.ResumeId,
            skill.Name,
            skill.Level,
            skill.Order);
    }
}