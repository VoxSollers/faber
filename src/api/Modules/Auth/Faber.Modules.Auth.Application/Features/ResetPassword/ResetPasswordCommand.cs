using ErrorOr;
using Faber.Modules.Identity.PublicApi.Shared;
using FastEndpoints;

namespace Faber.Modules.Auth.Application.Features.ResetPassword;

public record ResetPasswordCommand(VerificationKey VerificationKey, string NewPassword) : ICommand<ErrorOr<Success>>;