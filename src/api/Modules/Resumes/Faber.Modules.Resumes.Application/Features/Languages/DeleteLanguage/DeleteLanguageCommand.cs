using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Languages.DeleteLanguage;

public record DeleteLanguageCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<bool>>;