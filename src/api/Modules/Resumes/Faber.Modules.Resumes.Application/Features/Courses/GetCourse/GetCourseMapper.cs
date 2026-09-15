using Faber.Modules.Resumes.Domain.Entities;

namespace Faber.Modules.Resumes.Application.Features.Courses.GetCourse;

public static class GetCourseMapper
{
    public static GetCourseCommand MapToCommand(this GetCourseRequest request)
    {
        return new GetCourseCommand(request.ResumeId, request.Id);
    }

    public static GetCourseResponse MapToResponse(this Course course)
    {
        return new GetCourseResponse(
            course.Id,
            course.ResumeId,
            course.School,
            course.Name,
            course.StartDate,
            course.EndDate,
            course.Description,
            course.Order);
    }
}