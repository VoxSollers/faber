using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Projects.GetProject;

public record GetProjectCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<GetProjectResponse>>;