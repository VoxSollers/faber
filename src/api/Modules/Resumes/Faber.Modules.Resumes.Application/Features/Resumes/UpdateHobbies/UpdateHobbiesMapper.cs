namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateHobbies;

public static class UpdateHobbiesMapper
{
    public static UpdateHobbiesCommand MapToCommand(this UpdateHobbiesRequest request, Guid userId)
    {
        return new UpdateHobbiesCommand(request.ResumeId, userId, request.Hobbies);
    }
}