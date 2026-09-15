using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Resumes.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Persons.GetPersonByResume;

public class GetPersonByResumeEndpoint(ILogger<GetPersonByResumeEndpoint> logger)
    : Endpoint<GetPersonByResumeRequest, Results<Ok<GetPersonByResumeResponse>, NotFound>>
{
    public override void Configure()
    {
        Get("");
        Group<PersonsSubGroup>();
        Version(1);
        Policies("ResumeOwnerPolicy");
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthenticatedDefault));
    }

    public override async Task<Results<Ok<GetPersonByResumeResponse>, NotFound>> ExecuteAsync(
        GetPersonByResumeRequest req,
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
