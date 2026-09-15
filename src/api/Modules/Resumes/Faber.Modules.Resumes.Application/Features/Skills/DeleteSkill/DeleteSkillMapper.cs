namespace Faber.Modules.Resumes.Application.Features.Skills.DeleteSkill;

public static class DeleteSkillMapper
{
    public static DeleteSkillCommand MapToCommand(this DeleteSkillRequest request)
    {
        return new DeleteSkillCommand(request.ResumeId, request.Id);
    }
}