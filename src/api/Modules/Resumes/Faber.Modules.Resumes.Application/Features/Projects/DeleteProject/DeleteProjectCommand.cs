using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Projects.DeleteProject;

public record DeleteProjectCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<bool>>;