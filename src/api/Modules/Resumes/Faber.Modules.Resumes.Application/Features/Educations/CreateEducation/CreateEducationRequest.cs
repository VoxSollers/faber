namespace Faber.Modules.Resumes.Application.Features.Educations.CreateEducation;

public record CreateEducationRequest(
    Guid ResumeId,
    string? School,
    string? Degree,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? City,
    string? Description);