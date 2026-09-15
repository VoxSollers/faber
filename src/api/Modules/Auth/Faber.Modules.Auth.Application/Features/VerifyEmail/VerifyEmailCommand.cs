using ErrorOr;
using Faber.Modules.Identity.PublicApi.Shared;
using FastEndpoints;

namespace Faber.Modules.Auth.Application.Features.VerifyEmail;

public record VerifyEmailCommand(VerificationKey VerificationKey) : ICommand<ErrorOr<VerifyEmailResponse>>;