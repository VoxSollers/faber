namespace Faber.Modules.Resumes.Application.Features.Educations.GetEducation;

public static class GetEducationMapper
{
    public static GetEducationCommand MapToCommand(this GetEducationRequest request)
    {
        return new GetEducationCommand(request.ResumeId, request.Id);
    }

    public static GetEducationResponse MapToResponse(this Domain.Entities.Education education)
    {
        return new GetEducationResponse(
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