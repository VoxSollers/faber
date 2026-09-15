namespace Faber.Modules.Resumes.Application.Features.Educations.GetAllEducations;

public static class GetAllEducationsMapper
{
    public static GetAllEducationsCommand MapToCommand(this GetAllEducationsRequest request)
    {
        return new GetAllEducationsCommand(request.ResumeId);
    }

    public static GetAllEducationsItem MapToItem(this Domain.Entities.Education education)
    {
        return new GetAllEducationsItem(
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