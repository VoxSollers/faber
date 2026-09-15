using Faber.Modules.Resumes.Domain.Entities;

namespace Faber.Modules.Resumes.Application.Features.Courses.CreateCourse;

public static class CreateCourseMapper
{
    public static CreateCourseCommand MapToCommand(this CreateCourseRequest request)
    {
        return new CreateCourseCommand(
            request.ResumeId,
            request.School,
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Description);
    }

    public static CreateCourseResponse MapToResponse(this Course course)
    {
        return new CreateCourseResponse(
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