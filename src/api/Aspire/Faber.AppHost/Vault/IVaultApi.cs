using Refit;

namespace Faber.AppHost.Vault;

public interface IVaultApi
{
    [Get("/v1/sys/health")]
    Task<IApiResponse<VaultHealthResponse>> GetHealthAsync(
        [Query] bool standbyok = true,
        CancellationToken ct = default);

    [Get("/v1/sys/seal-status")]
    Task<IApiResponse<VaultSealStatusResponse>> GetSealStatusAsync(
        CancellationToken ct = default);

    [Post("/v1/sys/unseal")]
    Task<IApiResponse<VaultSealStatusResponse>> UnsealAsync(
        [Body] VaultUnsealRequest request,
        CancellationToken ct = default);
}