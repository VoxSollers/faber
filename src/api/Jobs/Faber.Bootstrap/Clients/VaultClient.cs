using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Faber.Bootstrap;
using Microsoft.Extensions.Options;

namespace Faber.Bootstrap.Clients;

public sealed class VaultClient(HttpClient httpClient, IOptions<BootstrapOptions> options)
{
    private readonly BootstrapOptions _options = options.Value;

    public async Task EnsureKvV2MountAsync(CancellationToken cancellationToken)
    {
        using var request = CreateRequest(HttpMethod.Get, "v1/sys/mounts");
        using var response = await httpClient.SendAsync(request, cancellationToken);
        ResponseValidation.EnsureSuccess(response, "list Vault secret mounts");
        using var document = await JsonDocument.ParseAsync(
            await response.Content.ReadAsStreamAsync(cancellationToken),
            cancellationToken: cancellationToken);

        if (document.RootElement.TryGetProperty($"{_options.VaultMountPoint}/", out var mount))
        {
            var type = mount.GetProperty("type").GetString();
            var version = mount.TryGetProperty("options", out var mountOptions)
                && mountOptions.TryGetProperty("version", out var versionElement)
                    ? versionElement.GetString()
                    : null;
            if (type != "kv" || version != "2")
            {
                throw new InvalidOperationException(
                    $"Vault mount '{_options.VaultMountPoint}' exists but is not KV v2.");
            }

            return;
        }

        using var createRequest = CreateRequest(HttpMethod.Post, $"v1/sys/mounts/{Uri.EscapeDataString(_options.VaultMountPoint)}");
        createRequest.Content = JsonContent.Create(new { type = "kv", options = new { version = "2" } });
        using var createResponse = await httpClient.SendAsync(createRequest, cancellationToken);
        ResponseValidation.EnsureSuccess(createResponse, "enable the Vault KV v2 mount");
    }

    public async Task WriteSecretAsync(
        string path,
        IReadOnlyDictionary<string, string> values,
        CancellationToken cancellationToken)
    {
        using var request = CreateRequest(
            HttpMethod.Post,
            $"v1/{Uri.EscapeDataString(_options.VaultMountPoint)}/data/{Uri.EscapeDataString(path)}");
        request.Content = JsonContent.Create(new { data = values });
        using var response = await httpClient.SendAsync(request, cancellationToken);
        ResponseValidation.EnsureSuccess(response, $"write Vault secret '{path}'");
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, path);
        request.Headers.Add("X-Vault-Token", _options.VaultToken);
        return request;
    }
}
