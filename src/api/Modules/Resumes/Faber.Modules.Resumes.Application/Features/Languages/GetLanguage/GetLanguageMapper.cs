namespace Faber.Modules.Resumes.Application.Features.Languages.GetLanguage;

public static class GetLanguageMapper
{
    public static GetLanguageCommand MapToCommand(this GetLanguageRequest request)
    {
        return new GetLanguageCommand(request.ResumeId, request.Id);
    }

    public static GetLanguageResponse MapToResponse(this Domain.Entities.Language language)
    {
        return new GetLanguageResponse(
            language.Id,
            language.ResumeId,
            language.Name,
            language.Level,
            language.Order);
    }
}