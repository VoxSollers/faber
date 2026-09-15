using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Resumes.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Educations.DeleteEducation;

public class DeleteEducationEndpoint(ILogger<DeleteEducationEndpoint> logger)
    : Endpoint<DeleteEducationRequest, Results<NoContent, NotFound>>
{
    public override void Configure()
    {
        Delete("{Id}");
        Group<EducationsSubGroup>();
        Policies("ResumeOwnerPolicy");
        Version(1);
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthenticatedDefault));
    }

    public override async Task<Results<NoContent, NotFound>> ExecuteAsync(
        DeleteEducationRequest req,
        CancellationToken ct)
    {
        var path = HttpContext.Request.Path.Value;
        logger.LogInformation("[HTTP DELETE] {Path} started", path);

        var result = await req.MapToCommand().ExecuteAsync(ct);

        if (result.IsError)
        {
            logger.LogWarning("[HTTP DELETE] {Path} failed: {Error}", path, result.FirstError.Description);

            return TypedResults.NotFound();
        }

        logger.LogInformation("[HTTP DELETE] {Path} completed successfully", path);

        return TypedResults.NoContent();
    }
}
