namespace Faber.Modules.Resumes.Application.Features.Links.CreateLink;

public static class CreateLinkMapper
{
    public static CreateLinkCommand MapToCommand(this CreateLinkRequest request)
    {
        return new CreateLinkCommand(
            request.ResumeId,
            request.Label,
            request.Uri);
    }

    public static CreateLinkResponse MapToResponse(this Domain.Entities.Link link)
    {
        return new CreateLinkResponse(
            link.Id,
            link.ResumeId,
            link.Label,
            link.Uri,
            link.Order);
    }
}