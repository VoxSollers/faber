using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Resumes.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Educations.GetAllEducations;

public class GetAllEducationsEndpoint(ILogger<GetAllEducationsEndpoint> logger)
    : Endpoint<GetAllEducationsRequest, Results<Ok<GetAllEducationsResponse>, NotFound>>
{
    public override void Configure()
    {
        Get("");
        Group<EducationsSubGroup>();
        Policies("ResumeOwnerPolicy");
        Version(1);
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthenticatedDefault));
    }

    public override async Task<Results<Ok<GetAllEducationsResponse>, NotFound>> ExecuteAsync(
        GetAllEducationsRequest req,
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
