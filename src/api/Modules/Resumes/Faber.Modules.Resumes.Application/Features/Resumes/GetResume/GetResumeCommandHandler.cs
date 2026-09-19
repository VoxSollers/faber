using ErrorOr;
using Faber.Modules.Resumes.Application.Caching;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Resumes.GetResume;

public class GetResumeCommandHandler(
    IServiceScopeFactory scopeFactory,
    HybridCache cache,
    ILogger<GetResumeCommandHandler> logger)
    : ICommandHandler<GetResumeCommand, ErrorOr<ResumeResponse>>
{
    private const string HandlerName = nameof(GetResumeCommandHandler);

    public async Task<ErrorOr<ResumeResponse>> ExecuteAsync(GetResumeCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {ResumeId}", HandlerName, command.Id);

        var response = await cache.GetOrCreateAsync(
            ResumesCacheKeys.Resume(command.UserId, command.Id),
            async cacheCt =>
            {
                // HybridCache runs this factory once for every concurrent caller of the same key and
                // only cancels it once all callers have cancelled. If the factory closed over the first
                // caller's request-scoped DbContext, that caller's scope disposing (e.g. on request
                // abort) would dispose the DbContext out from under every other caller still waiting on
                // the shared factory. Owning a dedicated scope keeps the factory alive independent of
                // any single caller's request lifetime.
                await using var scope = scopeFactory.CreateAsyncScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ResumesDbContext>();

                var resume = await dbContext.Resumes
                    .AsNoTracking()
                    .Include(r => r.Person)
                    .Include(r => r.Experiences.OrderBy(e => e.Order))
                    .Include(r => r.Educations.OrderBy(e => e.Order))
                    .Include(r => r.Skills.OrderBy(s => s.Order))
                    .Include(r => r.Languages.OrderBy(l => l.Order))
                    .Include(r => r.Courses.OrderBy(c => c.Order))
                    .Include(r => r.Projects.OrderBy(p => p.Order))
                    .Include(r => r.Links.OrderBy(l => l.Order))
                    .FirstOrDefaultAsync(r => r.Id == command.Id && r.UserId == command.UserId, cacheCt);

                logger.LogInformation(
                    "[STEP] {HandlerName} | Resume {ResumeId} {Outcome} in the database",
                    HandlerName,
                    command.Id,
                    resume is null ? "not found" : "found");

                return resume?.ToResponse();
            },
            tags: [ResumesCacheKeys.ResumeTag(command.Id)],
            cancellationToken: ct);

        if (response is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.Id);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.Id}' was not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | Resume {ResumeId} found", HandlerName, command.Id);

        return response;
    }
}
