using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Resumes.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Experiences.CreateExperience;

public class CreateExperienceEndpoint(ILogger<CreateExperienceEndpoint> logger)
    : Endpoint<CreateExperienceRequest,
        Results<Ok<CreateExperienceResponse>, NotFound>>
{
    public override void Configure()
    {
        Post("");
        Group<ExperiencesSubGroup>();
        Version(1);
        Policies("ResumeOwnerPolicy");
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthenticatedDefault));
    }

    public override async Task<Results<Ok<CreateExperienceResponse>, NotFound>>
        ExecuteAsync(
            CreateExperienceRequest req,
            CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP POST] {Path} started", path);

        var result = await req.MapToCommand().ExecuteAsync(ct);

        if (result.IsError)
        {
            logger.LogWarning("[HTTP POST] {Path} failed: {Error}", path, result.FirstError.Description);

            return TypedResults.NotFound();
        }

        logger.LogInformation("[HTTP POST] {Path} completed successfully", path);

        return TypedResults.Ok(result.Value);
    }
}
