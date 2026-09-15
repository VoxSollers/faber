namespace Faber.Modules.Resumes.Application.Features.Educations.UpdateEducation;

public record UpdateEducationRequest(
    Guid Id,
    Guid ResumeId,
    string? School,
    string? Degree,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? City,
    string? Description);