namespace Faber.Modules.Resumes.Application.Features.Experiences.GetExperience;

public record GetExperienceResponse(
    Guid Id,
    Guid ResumeId,
    string? JobTitle,
    string? Employer,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? City,
    string? Description,
    int Order);