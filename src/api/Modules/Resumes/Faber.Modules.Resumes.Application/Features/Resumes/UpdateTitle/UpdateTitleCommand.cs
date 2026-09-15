using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateTitle;

public record UpdateTitleCommand(Guid Id, Guid UserId, string? Title) : ICommand<ErrorOr<bool>>;
