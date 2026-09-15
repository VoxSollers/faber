namespace Faber.Modules.Notifications.PublicApi.Contracts;

public record EmailSenderRequest(
    string To,
    string Subject,
    Dictionary<string, object?> Parameters,
    bool IsHtml = true);