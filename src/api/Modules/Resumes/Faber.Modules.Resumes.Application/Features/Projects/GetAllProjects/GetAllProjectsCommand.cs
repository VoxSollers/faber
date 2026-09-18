using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Projects.GetAllProjects;

public record GetAllProjectsCommand(Guid ResumeId) : ICommand<ErrorOr<GetAllProjectsResponse>>;