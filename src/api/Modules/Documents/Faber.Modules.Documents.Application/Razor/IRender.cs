using Microsoft.AspNetCore.Components;

namespace Faber.Modules.Documents.Application.Razor;

public interface IRender
{
    Task<string> RazorToHtmlAsync<TComponent>(
        Dictionary<string, object?> parameters,
        CancellationToken ct = default)
        where TComponent : IComponent;

    Task<Stream> ToPdfAsync(
        Func<Task<string>> html,
        CancellationToken ct = default);
}