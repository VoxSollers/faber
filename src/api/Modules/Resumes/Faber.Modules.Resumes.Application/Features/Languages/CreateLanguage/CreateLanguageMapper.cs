namespace Faber.Modules.Resumes.Application.Features.Languages.CreateLanguage;

public static class CreateLanguageMapper
{
    public static CreateLanguageCommand MapToCommand(this CreateLanguageRequest request)
    {
        return new CreateLanguageCommand(
            request.ResumeId,
            request.Name,
            request.Level);
    }

    public static CreateLanguageResponse MapToResponse(this Domain.Entities.Language language)
    {
        return new CreateLanguageResponse(
            language.Id,
            language.ResumeId,
            language.Name,
            language.Level,
            language.Order);
    }
}