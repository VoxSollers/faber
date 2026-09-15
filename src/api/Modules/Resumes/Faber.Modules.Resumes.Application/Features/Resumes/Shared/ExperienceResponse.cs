namespace Faber.Modules.Resumes.Application.Features.Resumes.Shared;

public record ExperienceResponse(
    Guid Id,
    string? JobTitle,
    string? Employer,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? City,
    string? Description,
    int Order);
