using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Resumes.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Courses.CreateCourse;

public class CreateCourseEndpoint(ILogger<CreateCourseEndpoint> logger)
    : Endpoint<CreateCourseRequest, Results<Ok<CreateCourseResponse>, NotFound>>
{
    public override void Configure()
    {
        Post("");
        Group<CoursesSubGroup>();
        Version(1);
        Policies("ResumeOwnerPolicy");
        Options(x => x.RequireRateLimiting(RateLimitPolicies.AuthenticatedDefault));
    }

    public override async Task<Results<Ok<CreateCourseResponse>, NotFound>> ExecuteAsync(
        CreateCourseRequest req,
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