using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Languages.GetLanguage;

public record GetLanguageCommand(Guid ResumeId, Guid Id) : ICommand<ErrorOr<GetLanguageResponse>>;