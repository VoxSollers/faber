namespace Faber.Modules.Resumes.Application.Features.Educations.CreateEducation;

public record CreateEducationResponse(
    Guid Id,
    Guid ResumeId,
    string? School,
    string? Degree,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? City,
    string? Description,
    int Order);