namespace Faber.Modules.Resumes.Application.Features.Languages.GetLanguage;

public record GetLanguageResponse(
    Guid Id,
    Guid ResumeId,
    string? Name,
    string? Level,
    int Order);