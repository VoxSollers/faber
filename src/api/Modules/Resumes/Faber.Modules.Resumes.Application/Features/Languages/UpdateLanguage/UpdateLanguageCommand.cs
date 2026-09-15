using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Languages.UpdateLanguage;

public record UpdateLanguageCommand(
    Guid Id,
    Guid ResumeId,
    string? Name,
    string? Level) : ICommand<ErrorOr<bool>>;