namespace Faber.Modules.Resumes.Application.Features.Skills.UpdateSkill;

public static class UpdateSkillMapper
{
    public static UpdateSkillCommand MapToCommand(this UpdateSkillRequest request)
    {
        return new UpdateSkillCommand(
            request.Id,
            request.ResumeId,
            request.Name,
            request.Level);
    }
}