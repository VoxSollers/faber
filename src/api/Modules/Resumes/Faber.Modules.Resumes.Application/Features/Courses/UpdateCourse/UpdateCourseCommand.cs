using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Courses.UpdateCourse;

public record UpdateCourseCommand(
    Guid Id,
    Guid ResumeId,
    string? School,
    string? Name,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description) : ICommand<ErrorOr<bool>>;