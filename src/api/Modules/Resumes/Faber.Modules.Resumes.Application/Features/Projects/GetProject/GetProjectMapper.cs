using Faber.Modules.Resumes.Domain.Entities;

namespace Faber.Modules.Resumes.Application.Features.Projects.GetProject;

public static class GetProjectMapper
{
    public static GetProjectCommand MapToCommand(this GetProjectRequest request)
    {
        return new GetProjectCommand(request.ResumeId, request.Id);
    }

    public static GetProjectResponse MapToResponse(this Project project)
    {
        return new GetProjectResponse(
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