using Faber.Modules.Resumes.Domain.Entities;

namespace Faber.Modules.Resumes.Application.Features.Courses.GetAllCourses;

public static class GetAllCoursesMapper
{
    public static GetAllCoursesCommand MapToCommand(this GetAllCoursesRequest request)
    {
        return new GetAllCoursesCommand(request.ResumeId);
    }

    public static GetAllCoursesItem MapToItem(this Course course)
    {
        return new GetAllCoursesItem(
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