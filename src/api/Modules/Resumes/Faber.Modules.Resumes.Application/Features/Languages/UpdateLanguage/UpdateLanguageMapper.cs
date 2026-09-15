namespace Faber.Modules.Resumes.Application.Features.Languages.UpdateLanguage;

public static class UpdateLanguageMapper
{
    public static UpdateLanguageCommand MapToCommand(this UpdateLanguageRequest request)
    {
        return new UpdateLanguageCommand(
            request.Id,
            request.ResumeId,
            request.Name,
            request.Level);
    }
}