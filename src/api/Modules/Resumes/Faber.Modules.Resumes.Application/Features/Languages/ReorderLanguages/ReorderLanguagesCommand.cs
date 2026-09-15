using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Languages.ReorderLanguages;

public record ReorderLanguagesCommand(
    Guid ResumeId,
    IReadOnlyList<Guid> OrderedIds) : ICommand<ErrorOr<bool>>;
