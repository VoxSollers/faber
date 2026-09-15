namespace Faber.Modules.Resumes.Application.Features.Links.CreateLink;

public record CreateLinkRequest(
    Guid ResumeId,
    string? Label,
    string? Uri);