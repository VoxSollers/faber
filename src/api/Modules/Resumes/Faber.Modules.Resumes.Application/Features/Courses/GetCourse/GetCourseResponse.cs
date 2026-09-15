namespace Faber.Modules.Resumes.Application.Features.Courses.GetCourse;

public record GetCourseResponse(
    Guid Id,
    Guid ResumeId,
    string? School,
    string? Name,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description,
    int Order);