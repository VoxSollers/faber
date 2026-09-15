using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Languages.GetAllLanguages;

public record GetAllLanguagesCommand(Guid ResumeId) : ICommand<ErrorOr<GetAllLanguagesResponse>>;