namespace Faber.Modules.Resumes.Application.Features.Courses.DeleteCourse;

public static class DeleteCourseMapper
{
    public static DeleteCourseCommand MapToCommand(this DeleteCourseRequest request)
    {
        return new DeleteCourseCommand(request.ResumeId, request.Id);
    }
}