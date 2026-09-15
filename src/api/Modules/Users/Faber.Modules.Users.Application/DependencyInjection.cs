using Faber.Modules.Users.PublicApi;
using Microsoft.Extensions.DependencyInjection;

namespace Faber.Modules.Users.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services)
    {
        services.AddScoped<IUserModuleApi, UserModuleApi>();

        return services;
    }
}