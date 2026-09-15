namespace Faber.Modules.Resumes.Application.Features.Courses.UpdateCourse;

public static class UpdateCourseMapper
{
    public static UpdateCourseCommand MapToCommand(this UpdateCourseRequest request)
    {
        return new UpdateCourseCommand(
            request.Id,
            request.ResumeId,
            request.School,
            request.Name,
            request.StartDate,
            request.EndDate,
            request.Description);
    }
}