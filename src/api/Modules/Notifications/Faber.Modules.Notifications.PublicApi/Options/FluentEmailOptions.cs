namespace Faber.Modules.Notifications.PublicApi.Options;

public class FluentEmailOptions
{
    public string SmtpServer { get; init; } = string.Empty;

    public string SmtpPort { get; init; } = string.Empty;

    public string FromEmail { get; init; } = string.Empty;

    public string FromName { get; init; } = string.Empty;

    public bool EnableSsl { get; init; } = false;

    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;
}