using Faber.Modules.Identity.PublicApi.Shared;

namespace Faber.Modules.Auth.Application.Features.VerifyEmail;

public record VerifyEmailRequest(CombinedKey CombinedKey);