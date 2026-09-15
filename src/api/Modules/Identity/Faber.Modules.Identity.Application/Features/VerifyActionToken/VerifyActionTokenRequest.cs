using Faber.Modules.Identity.Domain.Enums;
using Faber.Modules.Identity.PublicApi.Shared;

namespace Faber.Modules.Identity.Application.Features.VerifyActionToken;

public record VerifyActionTokenRequest(CombinedKey CombinedKey, ActionTokenType Type);