namespace Faber.Modules.Resumes.Application.Features.Projects.GetProject;

public record GetProjectResponse(
    Guid Id,
    Guid ResumeId,
    string? Tagline,
    string? Name,
    string? Url,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description,
    int Order);