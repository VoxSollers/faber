namespace Faber.Modules.Identity.PublicApi.Contracts;

public record VerifyActionTokenResponse(bool IsValid, string? Email = null, string Message = "");