using Faber.Modules.Identity.PublicApi.Shared;

namespace Faber.Modules.Identity.Application.Features.VerifyActionToken;

public static class VerifyActionTokenMapper
{
    public static VerifyActionTokenCommand MapToCommand(this VerifyActionTokenRequest request)
    {
        return new VerifyActionTokenCommand(request.CombinedKey.ToVerificationKey(), request.Type);
    }
}