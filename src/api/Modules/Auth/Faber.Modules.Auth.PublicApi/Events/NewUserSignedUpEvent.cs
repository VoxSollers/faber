using Faber.Modules.Identity.PublicApi.Shared;
using FastEndpoints;

namespace Faber.Modules.Auth.PublicApi.Events;

public sealed record NewUserSignedUpEvent(string Email, CombinedKey CombinedKey) : IEvent;