using Faber.Modules.Resumes.Domain.Entities;

namespace Faber.Modules.Resumes.Application.Features.Resumes.CreateResume;

public static class CreateResumeMapper
{
    public static CreateResumeCommand MapToCommand(this CreateResumeRequest request, Guid userId)
    {
        return new CreateResumeCommand(userId, request.Localization);
    }

    public static CreateResumeResponse MapToResponse(this Resume resume)
    {
        return new CreateResumeResponse(resume.Id, resume.CreatedAt, resume.Localization);
    }
}