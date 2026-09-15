using Faber.Modules.Identity.PublicApi.Shared;

namespace Faber.Modules.Auth.Application.Features.ResetPassword;

public record ResetPasswordRequest(
    CombinedKey CombinedKey,
    string NewPassword,
    string ConfirmNewPassword);