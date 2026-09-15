namespace Faber.Modules.Resumes.Application.Features.Courses.ReorderCourses;

public record ReorderCoursesRequest(
    Guid ResumeId,
    IReadOnlyList<Guid> OrderedIds);
