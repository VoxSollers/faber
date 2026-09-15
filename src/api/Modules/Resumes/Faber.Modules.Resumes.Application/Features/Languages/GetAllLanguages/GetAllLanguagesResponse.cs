namespace Faber.Modules.Resumes.Application.Features.Languages.GetAllLanguages;

public record GetAllLanguagesResponse(List<GetAllLanguagesItem> Items);

public record GetAllLanguagesItem(
    Guid Id,
    Guid ResumeId,
    string? Name,
    string? Level,
    int Order);