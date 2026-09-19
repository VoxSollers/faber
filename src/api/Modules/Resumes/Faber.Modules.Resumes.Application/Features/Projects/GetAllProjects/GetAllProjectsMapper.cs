using Faber.Modules.Resumes.Domain.Entities;

namespace Faber.Modules.Resumes.Application.Features.Projects.GetAllProjects;

public static class GetAllProjectsMapper
{
    public static GetAllProjectsCommand MapToCommand(this GetAllProjectsRequest request)
    {
        return new GetAllProjectsCommand(request.ResumeId);
    }

    public static GetAllProjectsItem MapToItem(this Project project)
    {
        return new GetAllProjectsItem(
            project.Id,
            project.ResumeId,
            project.Tagline,
            project.Name,
            project.Url,
            project.StartDate,
            project.EndDate,
            project.Description,
            project.Order);
    }
}