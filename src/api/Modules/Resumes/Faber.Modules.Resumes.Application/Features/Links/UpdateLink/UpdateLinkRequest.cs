namespace Faber.Modules.Resumes.Application.Features.Links.UpdateLink;

public record UpdateLinkRequest(
    Guid Id,
    Guid ResumeId,
    string? Label,
    string? Uri);