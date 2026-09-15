using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Resumes.GetResume;

public record GetResumeCommand(Guid Id, Guid UserId) : ICommand<ErrorOr<ResumeResponse>>;
