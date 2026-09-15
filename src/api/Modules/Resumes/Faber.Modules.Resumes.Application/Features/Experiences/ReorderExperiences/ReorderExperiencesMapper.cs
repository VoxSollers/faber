namespace Faber.Modules.Resumes.Application.Features.Experiences.ReorderExperiences;

public static class ReorderExperiencesMapper
{
    public static ReorderExperiencesCommand MapToCommand(this ReorderExperiencesRequest request)
    {
        return new ReorderExperiencesCommand(request.ResumeId, request.OrderedIds);
    }
}
