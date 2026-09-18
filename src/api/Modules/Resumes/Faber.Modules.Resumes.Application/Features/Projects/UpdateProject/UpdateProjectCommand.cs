using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Projects.UpdateProject;

public record UpdateProjectCommand(
    Guid Id,
    Guid ResumeId,
    string? Role,
    string? Name,
    string? Url,
    DateOnly? StartDate,
    DateOnly? EndDate,
    string? Description) : ICommand<ErrorOr<bool>>;