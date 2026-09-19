namespace Faber.Modules.Resumes.Application.Features.Projects.UpdateProject;

public static class UpdateProjectMapper
{
    public static UpdateProjectCommand MapToCommand(this UpdateProjectRequest request)
    {
        return new UpdateProjectCommand(
            request.Id,
            request.ResumeId,
            request.Tagline,
            request.Name,
            request.Url,
            request.StartDate,
            request.EndDate,
            request.Description);
    }
}