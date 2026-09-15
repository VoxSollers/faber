namespace Faber.Modules.Resumes.Application.Features.Links.DeleteLink;

public static class DeleteLinkMapper
{
    public static DeleteLinkCommand MapToCommand(this DeleteLinkRequest request)
    {
        return new DeleteLinkCommand(request.ResumeId, request.Id);
    }
}