namespace Faber.Modules.Resumes.Application.Features.Languages.UpdateLanguage;

public record UpdateLanguageRequest(
    Guid Id,
    Guid ResumeId,
    string? Name,
    string? Level);