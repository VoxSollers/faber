using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Courses.CreateCourse;

public record CreateCourseCommand(
    Guid ResumeId,
    string? School,
    string? Name,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description) : ICommand<ErrorOr<CreateCourseResponse>>;