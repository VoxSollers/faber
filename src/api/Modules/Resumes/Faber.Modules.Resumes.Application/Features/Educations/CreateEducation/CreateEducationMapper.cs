namespace Faber.Modules.Resumes.Application.Features.Educations.CreateEducation;

public static class CreateEducationMapper
{
    public static CreateEducationCommand MapToCommand(this CreateEducationRequest request)
    {
        return new CreateEducationCommand(
            request.ResumeId,
            request.School,
            request.Degree,
            request.StartDate,
            request.EndDate,
            request.City,
            request.Description);
    }

    public static CreateEducationResponse MapToResponse(this Domain.Entities.Education education)
    {
        return new CreateEducationResponse(
            education.Id,
            education.ResumeId,
            education.School,
            education.Degree,
            education.StartDate,
            education.EndDate,
            education.City,
            education.Description,
            education.Order);
    }
}