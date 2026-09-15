namespace Faber.Modules.Identity.PublicApi.Contracts;

public record VerifyEmailResponse(bool IsSuccess, string? Message = null);