using Faber.Modules.Documents.Application.Razor;
using Faber.Modules.Documents.Application.Services;
using Faber.Modules.Documents.PublicApi;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Faber.Modules.Documents.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDocumentsModule(this IServiceCollection services)
    {
        services.AddSingleton<HtmlRenderer>();
        services.AddSingleton<PlaywrightService>();
        services.AddScoped<IRender, Render>();
        services.AddScoped<IDocumentsModuleApi, DocumentsModuleApi>();

        return services;
    }

    public static async Task UseDocumentsModuleAsync(this IHost app)
    {
        var playwrightService = app.Services.GetRequiredService<PlaywrightService>();
        await playwrightService.InitBrowserAsync();
    }
}