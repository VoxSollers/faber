using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Links.ReorderLinks;

public record ReorderLinksCommand(
    Guid ResumeId,
    IReadOnlyList<Guid> OrderedIds) : ICommand<ErrorOr<bool>>;
