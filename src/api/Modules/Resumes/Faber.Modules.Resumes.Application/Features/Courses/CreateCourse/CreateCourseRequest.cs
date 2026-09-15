namespace Faber.Modules.Resumes.Application.Features.Courses.CreateCourse;

public record CreateCourseRequest(
    Guid ResumeId,
    string? School,
    string? Name,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description);