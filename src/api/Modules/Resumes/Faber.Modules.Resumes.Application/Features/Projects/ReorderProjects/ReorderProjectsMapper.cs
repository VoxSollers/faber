namespace Faber.Modules.Resumes.Application.Features.Projects.ReorderProjects;

public static class ReorderProjectsMapper
{
    public static ReorderProjectsCommand MapToCommand(this ReorderProjectsRequest request)
    {
        return new ReorderProjectsCommand(request.ResumeId, request.OrderedIds);
    }
}
