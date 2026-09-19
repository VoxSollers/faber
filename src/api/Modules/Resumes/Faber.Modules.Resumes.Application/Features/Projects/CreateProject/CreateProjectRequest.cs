namespace Faber.Modules.Resumes.Application.Features.Projects.CreateProject;

public record CreateProjectRequest(
    Guid ResumeId,
    string? Tagline,
    string? Name,
    string? Url,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description);