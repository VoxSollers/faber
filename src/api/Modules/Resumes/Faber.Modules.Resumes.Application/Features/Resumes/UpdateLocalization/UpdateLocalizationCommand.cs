using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Resumes.UpdateLocalization;

public record UpdateLocalizationCommand(Guid Id, Guid UserId, string? Localization) : ICommand<ErrorOr<bool>>;