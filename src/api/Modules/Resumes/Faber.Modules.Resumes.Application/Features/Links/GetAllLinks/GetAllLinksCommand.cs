using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Links.GetAllLinks;

public record GetAllLinksCommand(Guid ResumeId) : ICommand<ErrorOr<GetAllLinksResponse>>;