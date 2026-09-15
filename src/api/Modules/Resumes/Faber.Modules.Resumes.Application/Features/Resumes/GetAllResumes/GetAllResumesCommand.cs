using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Resumes.GetAllResumes;

public record GetAllResumesCommand(Guid UserId) : ICommand<ErrorOr<GetAllResumesResponse>>;