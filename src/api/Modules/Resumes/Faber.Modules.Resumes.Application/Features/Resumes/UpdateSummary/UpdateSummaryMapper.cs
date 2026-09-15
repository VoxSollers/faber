namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateSummary;

public static class UpdateSummaryMapper
{
    public static UpdateSummaryCommand MapToCommand(this UpdateSummaryRequest request, Guid userId)
    {
        return new UpdateSummaryCommand(request.ResumeId, userId, request.Summary);
    }
}