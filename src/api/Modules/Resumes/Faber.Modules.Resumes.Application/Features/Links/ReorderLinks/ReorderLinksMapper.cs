namespace Faber.Modules.Resumes.Application.Features.Links.ReorderLinks;

public static class ReorderLinksMapper
{
    public static ReorderLinksCommand MapToCommand(this ReorderLinksRequest request)
    {
        return new ReorderLinksCommand(request.ResumeId, request.OrderedIds);
    }
}
