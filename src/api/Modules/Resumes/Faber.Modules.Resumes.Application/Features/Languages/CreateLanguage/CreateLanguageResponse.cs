namespace Faber.Modules.Resumes.Application.Features.Languages.CreateLanguage;

public record CreateLanguageResponse(
    Guid Id,
    Guid ResumeId,
    string? Name,
    string? Level,
    int Order);