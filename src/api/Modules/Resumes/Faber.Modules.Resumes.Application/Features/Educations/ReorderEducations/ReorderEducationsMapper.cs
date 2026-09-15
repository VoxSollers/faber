namespace Faber.Modules.Resumes.Application.Features.Educations.ReorderEducations;

public static class ReorderEducationsMapper
{
    public static ReorderEducationsCommand MapToCommand(this ReorderEducationsRequest request)
    {
        return new ReorderEducationsCommand(request.ResumeId, request.OrderedIds);
    }
}
