using Faber.Modules.Identity.Domain.Enums;
using Faber.Modules.Identity.PublicApi.Shared;
using FastEndpoints;

namespace Faber.Modules.Identity.Application.Features.VerifyActionToken;

public record VerifyActionTokenCommand(VerificationKey VerificationKey, ActionTokenType Type)
    : ICommand<VerifyActionTokenResponse>;