using Faber.Modules.Identity.PublicApi.Shared;

namespace Faber.Modules.Auth.Application.Features.ResetPassword;

public static class ResetPasswordMapper
{
    public static ResetPasswordCommand MapToCommand(this ResetPasswordRequest request)
    {
        return new ResetPasswordCommand(
            request.CombinedKey.ToVerificationKey(),
            request.NewPassword);
    }
}