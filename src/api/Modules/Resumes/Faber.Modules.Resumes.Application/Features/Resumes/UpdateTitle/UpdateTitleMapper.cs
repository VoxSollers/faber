namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateTitle;

public static class UpdateTitleMapper
{
    public static UpdateTitleCommand MapToCommand(this UpdateTitleRequest request, Guid userId)
    {
        return new UpdateTitleCommand(request.ResumeId, userId, request.Title);
    }
}
