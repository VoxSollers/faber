namespace Faber.Modules.Identity.PublicApi.Contracts;

public record ResetPasswordResponse(bool IsSuccess, string? Message = null);