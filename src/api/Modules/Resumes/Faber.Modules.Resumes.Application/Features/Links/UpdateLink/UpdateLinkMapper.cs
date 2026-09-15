namespace Faber.Modules.Resumes.Application.Features.Links.UpdateLink;

public static class UpdateLinkMapper
{
    public static UpdateLinkCommand MapToCommand(this UpdateLinkRequest request)
    {
        return new UpdateLinkCommand(
            request.Id,
            request.ResumeId,
            request.Label,
            request.Uri);
    }
}