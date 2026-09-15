using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Resumes.DeleteResume;

public record DeleteResumeCommand(Guid Id, Guid UserId) : ICommand<ErrorOr<bool>>;