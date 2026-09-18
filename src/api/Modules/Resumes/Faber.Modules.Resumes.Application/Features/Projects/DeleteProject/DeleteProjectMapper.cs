namespace Faber.Modules.Resumes.Application.Features.Projects.DeleteProject;

public static class DeleteProjectMapper
{
    public static DeleteProjectCommand MapToCommand(this DeleteProjectRequest request)
    {
        return new DeleteProjectCommand(request.ResumeId, request.Id);
    }
}