using Faber.Modules.Resumes.Domain.Entities;

namespace Faber.Modules.Resumes.Application.Features.Projects.CreateProject;

public static class CreateProjectMapper
{
    public static CreateProjectCommand MapToCommand(this CreateProjectRequest request)
    {
        return new CreateProjectCommand(
            request.ResumeId,
            request.Role,
            request.Name,
            request.Url,
            request.StartDate,
            request.EndDate,
            request.Description);
    }

    public static CreateProjectResponse MapToResponse(this Project project)
    {
        return new CreateProjectResponse(
            project.Id,
            project.ResumeId,
            project.Role,
            project.Name,
            project.Url,
            project.StartDate,
            project.EndDate,
            project.Description,
            project.Order);
    }
}