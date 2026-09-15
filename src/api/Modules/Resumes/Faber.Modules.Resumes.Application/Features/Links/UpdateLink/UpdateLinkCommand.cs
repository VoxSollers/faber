using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Links.UpdateLink;

public record UpdateLinkCommand(
    Guid Id,
    Guid ResumeId,
    string? Label,
    string? Uri) : ICommand<ErrorOr<bool>>;