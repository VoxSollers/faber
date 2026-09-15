namespace Faber.Modules.Resumes.Application.Features.Courses.ReorderCourses;

public static class ReorderCoursesMapper
{
    public static ReorderCoursesCommand MapToCommand(this ReorderCoursesRequest request)
    {
        return new ReorderCoursesCommand(request.ResumeId, request.OrderedIds);
    }
}
