using System.Diagnostics;
using System.Net;
using Faber.Modules.Resumes.Application.Features.Documents.DownloadDocument;
using Faber.Modules.Resumes.Application.Features.Documents.Shared;
using Faber.Modules.Resumes.Application.Features.Persons.CreatePerson;
using Faber.Modules.Resumes.Application.Features.Resumes.GetResume;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Application.Features.Resumes.UpdateTitle;
using Faber.Modules.Resumes.Application.Tests.Features.Shared;
using Faber.Modules.Resumes.Application.Tests.Features.Shared.Data;
using Faber.Testing.Shared.Http;
using FastEndpoints;
using FastEndpoints.Testing;
using Shouldly;

namespace Faber.Modules.Resumes.Application.Tests.Features.Resumes.Caching;

[Collection<CollectionRedisUnavailable>]
[Priority(92)]
public class RedisUnavailableTests(RedisUnavailableWebApp app) : TestBase
{
    /// <summary>
    /// Generously above expected latency but well under the StackExchange 5 s backlog default, so a
    /// regression to "queue requests until Redis times out" gets caught without a flaky assertion.
    /// </summary>
    private static readonly TimeSpan MaximumRequestDuration = TimeSpan.FromSeconds(3);

    [Fact]
    public async Task GetResume_RedisDown_ShouldReturnResume()
    {
        var ct = TestContext.Current.CancellationToken;
        var accessToken = await ResumesTestHelper.SignInAsFirstUserAsync(app.Client, ct);
        var resume = await ResumesTestHelper.CreateResumeAsync(app.Client, accessToken, ct: ct);

        var stopwatch = Stopwatch.StartNew();

        var (response, getResponse) = await app.Client
            .WithAuthToken(accessToken)
            .GETAsync<GetResumeEndpoint, GetResumeRequest, ResumeResponse>(new GetResumeRequest(resume.Id));

        stopwatch.Stop();

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        getResponse!.Id.ShouldBe(resume.Id);
        stopwatch.Elapsed.ShouldBeLessThan(MaximumRequestDuration);
    }

    [Fact]
    public async Task UpdateTitle_RedisDown_ShouldSucceedAndReadBackFresh()
    {
        var ct = TestContext.Current.CancellationToken;
        var accessToken = await ResumesTestHelper.SignInAsFirstUserAsync(app.Client, ct);
        var resume = await ResumesTestHelper.CreateResumeAsync(app.Client, accessToken, ct: ct);

        // Primes the L1 in-memory cache entry for this resume; with Redis down, invalidation
        // must still clear it, or the read-back below would serve the stale title.
        await app.Client
            .WithAuthToken(accessToken)
            .GETAsync<GetResumeEndpoint, GetResumeRequest, ResumeResponse>(new GetResumeRequest(resume.Id));

        var updateResponse = await app.Client
            .WithAuthToken(accessToken)
            .PUTAsync<UpdateTitleEndpoint, UpdateTitleRequest>(new UpdateTitleRequest(resume.Id, "Fresh Title"));

        updateResponse.StatusCode.ShouldBe(HttpStatusCode.NoContent);

        var (_, getResponse) = await app.Client
            .WithAuthToken(accessToken)
            .GETAsync<GetResumeEndpoint, GetResumeRequest, ResumeResponse>(new GetResumeRequest(resume.Id));

        getResponse!.Title.ShouldBe("Fresh Title");
    }

    [Fact]
    public async Task DownloadDocument_RedisDown_ShouldReturnPdf()
    {
        var ct = TestContext.Current.CancellationToken;
        var accessToken = await ResumesTestHelper.SignInAsFirstUserAsync(app.Client, ct);
        var resume = await ResumesTestHelper.CreateResumeAsync(app.Client, accessToken, ct: ct);

        await app.Client
            .WithAuthToken(accessToken)
            .POSTAsync<CreatePersonEndpoint, CreatePersonRequest, CreatePersonResponse>(
                new CreatePersonRequestFaker(resume.Id).Generate());

        var stopwatch = Stopwatch.StartNew();

        var downloadResponse = await app.Client
            .WithAuthToken(accessToken)
            .GETAsync<DownloadDocumentEndpoint, DocumentRequest>(new DocumentRequest(resume.Id));

        stopwatch.Stop();

        downloadResponse.StatusCode.ShouldBe(HttpStatusCode.OK);
        stopwatch.Elapsed.ShouldBeLessThan(MaximumRequestDuration);
    }
}
