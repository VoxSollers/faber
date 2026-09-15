namespace Faber.Modules.Resumes.Application.Features.Links.GetAllLinks;

public static class GetAllLinksMapper
{
    public static GetAllLinksCommand MapToCommand(this GetAllLinksRequest request)
    {
        return new GetAllLinksCommand(request.ResumeId);
    }

    public static GetAllLinksItem MapToItem(this Domain.Entities.Link link)
    {
        return new GetAllLinksItem(
            link.Id,
            link.ResumeId,
            link.Label,
            link.Uri,
            link.Order);
    }
}