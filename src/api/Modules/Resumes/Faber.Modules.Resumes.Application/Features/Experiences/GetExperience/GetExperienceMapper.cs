namespace Faber.Modules.Resumes.Application.Features.Experiences.GetExperience;

public static class GetExperienceMapper
{
    public static GetExperienceCommand MapToCommand(this GetExperienceRequest request)
    {
        return new GetExperienceCommand(request.ResumeId, request.Id);
    }

    public static GetExperienceResponse MapToResponse(this Domain.Entities.Experience experience)
    {
        return new GetExperienceResponse(
            experience.Id,
            experience.ResumeId,
            experience.JobTitle,
            experience.Employer,
            experience.StartDate,
            experience.EndDate,
            experience.City,
            experience.Description,
            experience.Order);
    }
}