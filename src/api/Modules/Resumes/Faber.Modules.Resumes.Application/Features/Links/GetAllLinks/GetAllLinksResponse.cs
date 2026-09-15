namespace Faber.Modules.Resumes.Application.Features.Links.GetAllLinks;

public record GetAllLinksResponse(List<GetAllLinksItem> Items);

public record GetAllLinksItem(
    Guid Id,
    Guid ResumeId,
    string? Label,
    string? Uri,
    int Order);