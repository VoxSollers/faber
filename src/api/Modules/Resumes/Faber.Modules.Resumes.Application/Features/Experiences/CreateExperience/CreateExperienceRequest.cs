namespace Faber.Modules.Resumes.Application.Features.Experiences.CreateExperience;

public record CreateExperienceRequest(
    Guid ResumeId,
    string? JobTitle,
    string? Employer,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? City,
    string? Description);