namespace Faber.Modules.Resumes.Application.Features.Experiences.GetAllExperiences;

public static class GetAllExperiencesMapper
{
    public static GetAllExperiencesCommand MapToCommand(
        this GetAllExperiencesRequest request)
    {
        return new GetAllExperiencesCommand(request.ResumeId);
    }

    public static GetAllExperiencesItem MapToItem(this Domain.Entities.Experience experience)
    {
        return new GetAllExperiencesItem(
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