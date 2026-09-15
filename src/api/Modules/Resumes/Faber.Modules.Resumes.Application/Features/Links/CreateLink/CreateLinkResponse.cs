namespace Faber.Modules.Resumes.Application.Features.Links.CreateLink;

public record CreateLinkResponse(
    Guid Id,
    Guid ResumeId,
    string? Label,
    string? Uri,
    int Order);