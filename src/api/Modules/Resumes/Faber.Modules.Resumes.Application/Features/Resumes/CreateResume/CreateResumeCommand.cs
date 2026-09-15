using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Resumes.CreateResume;

public record CreateResumeCommand(Guid UserId, string? Localization) : ICommand<ErrorOr<CreateResumeResponse>>;