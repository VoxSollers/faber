namespace Faber.Modules.Resumes.Application.Features.Courses.CreateCourse;

public record CreateCourseResponse(
    Guid Id,
    Guid ResumeId,
    string? School,
    string? Name,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description,
    int Order);