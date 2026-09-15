using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateSummary;

public record UpdateSummaryCommand(Guid Id, Guid UserId, string? Summary) : ICommand<ErrorOr<bool>>;