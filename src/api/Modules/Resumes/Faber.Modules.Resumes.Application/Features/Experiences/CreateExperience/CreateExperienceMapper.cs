namespace Faber.Modules.Resumes.Application.Features.Experiences.CreateExperience;

public static class CreateExperienceMapper
{
    public static CreateExperienceCommand MapToCommand(this CreateExperienceRequest request)
    {
        return new CreateExperienceCommand(
            request.ResumeId,
            request.JobTitle,
            request.Employer,
            request.StartDate,
            request.EndDate,
            request.City,
            request.Description);
    }

    public static CreateExperienceResponse MapToResponse(
        this Domain.Entities.Experience experience)
    {
        return new CreateExperienceResponse(
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