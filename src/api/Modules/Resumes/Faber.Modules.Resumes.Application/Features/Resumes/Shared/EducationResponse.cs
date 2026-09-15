namespace Faber.Modules.Resumes.Application.Features.Resumes.Shared;

public record EducationResponse(
    Guid Id,
    string? School,
    string? Degree,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? City,
    string? Description,
    int Order);
