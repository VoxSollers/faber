namespace Faber.Modules.Resumes.Application.Features.Languages.ReorderLanguages;

public record ReorderLanguagesRequest(
    Guid ResumeId,
    IReadOnlyList<Guid> OrderedIds);
