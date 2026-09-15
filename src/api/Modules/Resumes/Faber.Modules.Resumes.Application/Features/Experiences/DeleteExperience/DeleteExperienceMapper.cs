namespace Faber.Modules.Resumes.Application.Features.Experiences.DeleteExperience;

public static class DeleteExperienceMapper
{
    public static DeleteExperienceCommand MapToCommand(this DeleteExperienceRequest request)
    {
        return new DeleteExperienceCommand(request.ResumeId, request.Id);
    }
}