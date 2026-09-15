namespace Faber.Modules.Resumes.Application.Features.Resumes.DeleteResume;

public static class DeleteResumeMapper
{
    public static DeleteResumeCommand MapToCommand(this DeleteResumeRequest request, Guid userId)
    {
        return new DeleteResumeCommand(request.Id, userId);
    }
}