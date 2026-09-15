using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace Faber.Api.Tests;

/// <summary>
/// TestServer connections have no <c>RemoteIpAddress</c>. This filter runs before the app's own
/// middleware (including ForwardedHeaders) and sets the connection IP from the X-Test-Client-Ip
/// header, letting tests simulate distinct clients and proxies.
/// </summary>
public class TestClientIpStartupFilter : IStartupFilter
{
    public const string HeaderName = "X-Test-Client-Ip";

    public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
    {
        return app =>
        {
            app.Use(async (context, nextMiddleware) =>
            {
                if (context.Request.Headers.TryGetValue(HeaderName, out var value) &&
                    IPAddress.TryParse(value.ToString(), out var address))
                {
                    context.Connection.RemoteIpAddress = address;
                }

                await nextMiddleware(context);
            });

            next(app);
        };
    }
}
