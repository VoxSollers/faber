using Faber.Modules.Auth.Application.Features.Shared.Requests;
using Faber.Modules.Auth.Domain.Enums;
using Faber.Modules.Common.PublicApi;
using FastEndpoints;

namespace Faber.Modules.Auth.Application.Features.Shared;

public abstract class EndpointWithRefreshToken<TResponse> : Endpoint<RefreshTokenRequest, TResponse>
{
    protected string? GetRefreshToken(RefreshTokenRequest request)
    {
        return Enum.TryParse(request.ClientType, out ClientType client) switch
        {
            true when client == ClientType.Web => HttpContext.GetCookiesRefreshToken(),
            true when client == ClientType.Mobile => request.RefreshToken,
            _ => string.Empty
        };
    }
}