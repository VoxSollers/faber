using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Links.GetLink;

public record GetLinkCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<GetLinkResponse>>;