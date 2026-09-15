using Faber.Modules.Auth.Application.Features.Shared.Constants;
using FastEndpoints;

namespace Faber.Modules.Auth.Application.Features.Shared.Requests;

public record RefreshTokenRequest(
    string? RefreshToken,
    [property: FromHeader(HeaderKeys.ClientType)] string ClientType);