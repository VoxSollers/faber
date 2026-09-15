using Faber.Modules.Common.PublicApi.RateLimiting;
using Faber.Modules.Documents.Application.Templates;
using Faber.Modules.Documents.PublicApi;
using Faber.Modules.Resumes.Application.Features.Documents.Shared;
using Faber.Modules.Resumes.Application.Groups;
using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Documents.DownloadDocument;

public class DownloadDocumentEndpoint(
    IDocumentsModuleApi documentsModuleApi,
    ILogger<DownloadDocumentEndpoint> logger)
    : Endpoint<DocumentRequest, Results<FileStreamHttpResult, NotFound>>
{
    public override void Configure()
    {
        Get("{ResumeId}/download");
        Group<ResumesGroup>();
        Policies("ResumeOwnerPolicy");
        Version(1);
        Options(x => x.RequireRateLimiting(RateLimitPolicies.ExpensiveResource));
    }

    public override async Task<Results<FileStreamHttpResult, NotFound>> ExecuteAsync(
        DocumentRequest req,
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

        var resume = result.Value;
        var parameters = new Dictionary<string, object?> { { "Resume", resume } };

        var stream = await documentsModuleApi.RenderToPdfAsync<FirstTemplate>(parameters, ct);
        var fileName = $"{resume.Person?.JobTitle ?? "resume"}.pdf";

        logger.LogInformation("[HTTP GET] {Path} completed successfully", path);

        return TypedResults.File(stream, "application/pdf", fileName);
    }
}