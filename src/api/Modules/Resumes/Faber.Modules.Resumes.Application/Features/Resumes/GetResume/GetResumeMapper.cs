namespace Faber.Modules.Resumes.Application.Features.Resumes.GetResume;

public static class GetResumeMapper
{
    public static GetResumeCommand MapToCommand(this GetResumeRequest request, Guid userId)
    {
        return new GetResumeCommand(request.Id, userId);
    }
}
