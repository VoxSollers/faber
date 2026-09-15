using ErrorOr;
using Faber.Modules.Auth.Application.Features.ForgotPassword.RateLimiting;
using Faber.Modules.Auth.Application.Groups;
using Faber.Modules.Common.PublicApi.RateLimiting;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Auth.Application.Features.ForgotPassword;

public class ForgotPasswordEndpoint(ILogger<ForgotPasswordEndpoint> logger, ITargetEmailRateLimiter emailRateLimiter)
    : Endpoint<ForgotPasswordRequest, Results<NoContent, BadRequest<Error>, ProblemHttpResult>>
{
    /// <summary>
    /// Value of the <c>policy</c> tag on the shared rejection counter when the per-target-email bucket
    /// rejects. Deliberately distinct from the per-IP <c>auth-password-reset</c> HTTP policy and from
    /// Notifications' <c>outbound-email</c> send-layer throttle, so the dashboard can attribute a
    /// rejection to the exact layer that fired.
    /// </summary>
    public const string MetricPolicy = "auth-forgot-password-target-email";

    public override void Configure()
    {
        Post("forgot-password");
        Group<AuthGroup>();
        Version(1);
        AllowAnonymous();
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthPasswordReset));
    }

    public override async Task<Results<NoContent, BadRequest<Error>, ProblemHttpResult>> ExecuteAsync(
        ForgotPasswordRequest request,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;

        // Checked purely on the raw request email, before any existence lookup: throttling must
        // never correlate with whether the account is real (enumeration-safety, issue #333).
        if (!emailRateLimiter.TryAcquire(request.Email))
        {
            RateLimitingMetrics.RecordRejection(MetricPolicy);

            logger.LogWarning("[HTTP POST] {Path} throttled: per-email limit exceeded", path);

            return RateLimitRejection.Problem(HttpContext, emailRateLimiter.RetryAfterSeconds);
        }

        logger.LogInformation("[HTTP POST] {Path} started for {Email}", path, request.Email);
        var result = await request.MapToCommand().ExecuteAsync(ct);

        if (result.IsError)
        {
            logger.LogWarning(
                "[HTTP POST] {Path} failed for {Email}: {Error}",
                path,
                request.Email,
                result.FirstError.Description);

            return TypedResults.BadRequest(result.FirstError);
        }

        logger.LogInformation("[HTTP POST] {Path} completed successfully for {Email}", path, request.Email);

        return TypedResults.NoContent();
    }
}
