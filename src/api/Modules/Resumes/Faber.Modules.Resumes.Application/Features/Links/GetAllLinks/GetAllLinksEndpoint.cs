using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Resumes.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Links.GetAllLinks;

public class GetAllLinksEndpoint(ILogger<GetAllLinksEndpoint> logger)
    : Endpoint<GetAllLinksRequest, Results<Ok<GetAllLinksResponse>, NotFound>>
{
    public override void Configure()
    {
        Get("");
        Group<LinksSubGroup>();
        Policies("ResumeOwnerPolicy");
        Version(1);
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthenticatedDefault));
    }

    public override async Task<Results<Ok<GetAllLinksResponse>, NotFound>> ExecuteAsync(
        GetAllLinksRequest req,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP GET] {Path} started", path);

        var result = await req.MapToCommand().ExecuteAsync(ct);

        if (result.IsError)
        {
            logger.LogWarning("[HTTP GET] {Path} failed: {Error}", path, result.FirstError.Description);

            return TypedResults.NotFound();
        }

        logger.LogInformation("[HTTP GET] {Path} completed successfully", path);

        return TypedResults.Ok(result.Value);
    }
}