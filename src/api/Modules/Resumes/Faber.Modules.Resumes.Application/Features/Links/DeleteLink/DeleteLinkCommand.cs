using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Links.DeleteLink;

public record DeleteLinkCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<bool>>;