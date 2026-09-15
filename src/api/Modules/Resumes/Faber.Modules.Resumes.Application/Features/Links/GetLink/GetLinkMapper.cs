namespace Faber.Modules.Resumes.Application.Features.Links.GetLink;

public static class GetLinkMapper
{
    public static GetLinkCommand MapToCommand(this GetLinkRequest request)
    {
        return new GetLinkCommand(request.ResumeId, request.Id);
    }

    public static GetLinkResponse MapToResponse(this Domain.Entities.Link link)
    {
        return new GetLinkResponse(
            link.Id,
            link.ResumeId,
            link.Label,
            link.Uri,
            link.Order);
    }
}