namespace Faber.Modules.Resumes.Application.Features.Languages.DeleteLanguage;

public static class DeleteLanguageMapper
{
    public static DeleteLanguageCommand MapToCommand(this DeleteLanguageRequest request)
    {
        return new DeleteLanguageCommand(request.ResumeId, request.Id);
    }
}