using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Projects.CreateProject;

public record CreateProjectCommand(
    Guid ResumeId,
    string? Tagline,
    string? Name,
    string? Url,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description) : ICommand<ErrorOr<CreateProjectResponse>>;