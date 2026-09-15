namespace Faber.Modules.Resumes.Application.Features.Experiences.UpdateExperience;

public static class UpdateExperienceMapper
{
    public static UpdateExperienceCommand MapToCommand(this UpdateExperienceRequest request)
    {
        return new UpdateExperienceCommand(
            request.Id,
            request.ResumeId,
            request.JobTitle,
            request.Employer,
            request.StartDate,
            request.EndDate,
            request.City,
            request.Description);
    }
}