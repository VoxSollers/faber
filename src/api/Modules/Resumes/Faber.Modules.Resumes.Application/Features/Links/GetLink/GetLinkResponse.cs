namespace Faber.Modules.Resumes.Application.Features.Links.GetLink;

public record GetLinkResponse(
    Guid Id,
    Guid ResumeId,
    string? Label,
    string? Uri,
    int Order);