namespace Faber.Modules.Resumes.Application.Features.Languages.ReorderLanguages;

public static class ReorderLanguagesMapper
{
    public static ReorderLanguagesCommand MapToCommand(this ReorderLanguagesRequest request)
    {
        return new ReorderLanguagesCommand(request.ResumeId, request.OrderedIds);
    }
}
