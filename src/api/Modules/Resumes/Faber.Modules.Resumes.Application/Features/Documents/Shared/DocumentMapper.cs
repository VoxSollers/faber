namespace Faber.Modules.Resumes.Application.Features.Documents.Shared;

public static class DocumentMapper
{
    public static DocumentCommand MapToCommand(this DocumentRequest request, Guid userId)
    {
        return new DocumentCommand(request.ResumeId, userId);
    }
}