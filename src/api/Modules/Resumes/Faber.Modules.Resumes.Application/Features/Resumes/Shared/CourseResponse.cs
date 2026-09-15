namespace Faber.Modules.Resumes.Application.Features.Resumes.Shared;

public record CourseResponse(
    Guid Id,
    string? School,
    string? Name,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description,
    int Order);
