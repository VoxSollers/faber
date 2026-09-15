using ErrorOr;
using Faber.Modules.Notifications.PublicApi.Contracts;
using Microsoft.AspNetCore.Components;

namespace Faber.Modules.Communication.PublicApi;

/// <summary>
/// Sends a rendered Razor template to a single recipient. Returns a result rather than <c>void</c> so
/// that a send suppressed by the per-recipient rate limiter, or rejected by the transport, is
/// observable to the caller instead of being swallowed.
/// </summary>
public interface IEmailSender
{
    /// <summary>Renders <typeparamref name="TComponent"/> and sends it.</summary>
    /// <typeparam name="TComponent">The Razor component used as the message body template.</typeparam>
    /// <param name="request">Recipient, subject and template parameters.</param>
    /// <param name="cancellationToken">Cancels rendering and dispatch.</param>
    /// <returns><see cref="Result.Success"/> when handed to the transport, otherwise an error.</returns>
    public Task<ErrorOr<Success>> SendAsync<TComponent>(
        EmailSenderRequest request,
        CancellationToken cancellationToken = default)
        where TComponent : IComponent;
}
