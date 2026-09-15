namespace Faber.Modules.Resumes.Application.Features.Courses.UpdateCourse;

public record UpdateCourseRequest(
    Guid Id,
    Guid ResumeId,
    string? School,
    string? Name,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description);