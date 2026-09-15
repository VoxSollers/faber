namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateLocalization;

public static class UpdateLocalizationMapper
{
    public static UpdateLocalizationCommand MapToCommand(this UpdateLocalizationRequest request, Guid userId)
    {
        return new UpdateLocalizationCommand(request.ResumeId, userId, request.Localization);
    }
}