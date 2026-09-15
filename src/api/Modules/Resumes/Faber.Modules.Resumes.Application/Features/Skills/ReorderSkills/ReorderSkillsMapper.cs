namespace Faber.Modules.Resumes.Application.Features.Skills.ReorderSkills;

public static class ReorderSkillsMapper
{
    public static ReorderSkillsCommand MapToCommand(this ReorderSkillsRequest request)
    {
        return new ReorderSkillsCommand(request.ResumeId, request.OrderedIds);
    }
}
