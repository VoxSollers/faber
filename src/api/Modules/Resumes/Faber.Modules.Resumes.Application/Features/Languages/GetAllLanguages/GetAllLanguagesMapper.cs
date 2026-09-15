namespace Faber.Modules.Resumes.Application.Features.Languages.GetAllLanguages;

public static class GetAllLanguagesMapper
{
    public static GetAllLanguagesCommand MapToCommand(this GetAllLanguagesRequest request)
    {
        return new GetAllLanguagesCommand(request.ResumeId);
    }

    public static GetAllLanguagesItem MapToItem(this Domain.Entities.Language language)
    {
        return new GetAllLanguagesItem(
            language.Id,
            language.ResumeId,
            language.Name,
            language.Level,
            language.Order);
    }
}