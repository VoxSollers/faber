namespace Faber.Modules.Resumes.Application.Features.Projects.UpdateProject;

public record UpdateProjectRequest(
    Guid Id,
    Guid ResumeId,
    string? Tagline,
    string? Name,
    string? Url,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description);