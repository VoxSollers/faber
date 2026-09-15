using System.Net;
using System.Net.Mail;
using Faber.Modules.Communication.PublicApi;
using Faber.Modules.Notifications.Application.Email;
using Faber.Modules.Notifications.Application.Email.RateLimiting;
using Faber.Modules.Notifications.PublicApi.Options;
using Faber.Modules.Notifications.PublicApi.OptionsSetup;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Resend;

namespace Faber.Modules.Notifications.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsModule(
        this IServiceCollection services,
        IWebHostEnvironment environment)
    {
        services.ConfigureOptions<FluentEmailOptionsSetup>();

        // Registered unconditionally: the limiter is environment-agnostic and its Enabled flag already
        // follows the shared RateLimiting:Enabled switch, so test hosts resolve it without special-casing.
        // Consumed by ThrottledEmailSender, which decorates whichever transport the environment selected.
        services.ConfigureOptions<RecipientEmailRateLimitingOptionsSetup>();
        services.AddSingleton<IRecipientEmailRateLimiter, RecipientEmailRateLimiter>();

        if (environment.IsDevelopment())
        {
            var serviceProvider = services.BuildServiceProvider();
            var fluentEmailOptions = serviceProvider.GetRequiredService<IOptions<FluentEmailOptions>>().Value;

            services
                .AddFluentEmail(fluentEmailOptions.FromEmail, fluentEmailOptions.FromName)
                .AddRazorRenderer()
                .AddSmtpSender(
                    new SmtpClient(fluentEmailOptions.SmtpServer, int.Parse(fluentEmailOptions.SmtpPort))
                    {
                        EnableSsl = fluentEmailOptions.EnableSsl,
                        UseDefaultCredentials = string.IsNullOrEmpty(fluentEmailOptions.Username),
                        Credentials = new NetworkCredential(fluentEmailOptions.Username, fluentEmailOptions.Password)
                    });

            services.AddScoped<FluentEmailSender>();
            services.AddScoped<IEmailSender>(sp => new ThrottledEmailSender(
                sp.GetRequiredService<FluentEmailSender>(),
                sp.GetRequiredService<IRecipientEmailRateLimiter>(),
                sp.GetRequiredService<ILogger<ThrottledEmailSender>>()));
        }

        if (environment.IsProduction())
        {
            services.AddHttpClient<ResendClient>();
            services.ConfigureOptions<ResendClientOptionsSetup>();
            services.AddTransient<IResend, ResendClient>();
            services.AddScoped<ResendEmailSender>();
            services.AddScoped<IEmailSender>(sp => new ThrottledEmailSender(
                sp.GetRequiredService<ResendEmailSender>(),
                sp.GetRequiredService<IRecipientEmailRateLimiter>(),
                sp.GetRequiredService<ILogger<ThrottledEmailSender>>()));
        }

        return services;
    }
}