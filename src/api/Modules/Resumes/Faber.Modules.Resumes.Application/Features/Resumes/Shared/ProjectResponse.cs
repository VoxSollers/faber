namespace Faber.Modules.Resumes.Application.Features.Resumes.Shared;

public record ProjectResponse(
    Guid Id,
    string? Name,
    string? Tagline,
    string? Url,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description,
    int Order);
