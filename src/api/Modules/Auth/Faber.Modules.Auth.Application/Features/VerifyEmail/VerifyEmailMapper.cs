using Faber.Modules.Identity.PublicApi.Shared;

namespace Faber.Modules.Auth.Application.Features.VerifyEmail;

public static class VerifyEmailMapper
{
    public static VerifyEmailCommand MapToCommand(this VerifyEmailRequest request)
    {
        return new VerifyEmailCommand(request.CombinedKey.ToVerificationKey());
    }
}