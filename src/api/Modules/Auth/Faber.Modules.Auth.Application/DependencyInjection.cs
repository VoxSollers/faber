using Faber.Modules.Auth.Application.Features.ForgotPassword.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace Faber.Modules.Auth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthModule(this IServiceCollection services)
    {
        services.ConfigureOptions<TargetEmailRateLimitingOptionsSetup>();
        services.AddSingleton<ITargetEmailRateLimiter, TargetEmailRateLimiter>();

        return services;
    }
}