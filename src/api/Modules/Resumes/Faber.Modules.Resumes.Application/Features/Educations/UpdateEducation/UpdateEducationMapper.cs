namespace Faber.Modules.Resumes.Application.Features.Educations.UpdateEducation;

public static class UpdateEducationMapper
{
    public static UpdateEducationCommand MapToCommand(this UpdateEducationRequest request)
    {
        return new UpdateEducationCommand(
            request.Id,
            request.ResumeId,
            request.School,
            request.Degree,
            request.StartDate,
            request.EndDate,
            request.City,
            request.Description);
    }
}