using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Documents.Application.Razor.Extensions;
using Faber.Modules.Documents.Application.Templates;
using Faber.Modules.Documents.PublicApi;
using Faber.Modules.Resumes.Application.Features.Documents.Shared;
using Faber.Modules.Resumes.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Documents.GenerateDocument;

public class GenerateDocumentEndpoint(
    IDocumentsModuleApi documentsModuleApi,
    ILogger<GenerateDocumentEndpoint> logger)
    : Endpoint<DocumentRequest, Results<Ok<GenerateDocumentResponse>, NotFound>>
{
    public override void Configure()
    {
        Post("{ResumeId}/generate");
        Group<ResumesGroup>();
        Policies("ResumeOwnerPolicy");
        Version(1);
        Options(x => x.RequireRateLimiting(RateLimitPolicies.ExpensiveResource));
    }

    public override async Task<Results<Ok<GenerateDocumentResponse>, NotFound>> ExecuteAsync(
        DocumentRequest req,
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

        var resume = result.Value;
        var parameters = new Dictionary<string, object?> { { "Resume", resume } };

        var stream = await documentsModuleApi.RenderToPdfAsync<FirstTemplate>(parameters, ct);
        var base64String = await stream.ToBase64StringAsync();

        logger.LogInformation("[HTTP POST] {Path} completed successfully", path);

        return TypedResults.Ok(new GenerateDocumentResponse(base64String));
    }
}