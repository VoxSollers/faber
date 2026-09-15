namespace Faber.Modules.Resumes.Application.Features.Languages.CreateLanguage;

public record CreateLanguageRequest(
    Guid ResumeId,
    string? Name,
    string? Level);