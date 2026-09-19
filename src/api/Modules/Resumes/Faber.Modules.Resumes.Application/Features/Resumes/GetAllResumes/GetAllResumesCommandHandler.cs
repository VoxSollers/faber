using ErrorOr;
using Faber.Modules.Resumes.Application.Caching;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Resumes.GetAllResumes;

public class GetAllResumesCommandHandler(
    ResumesDbContext dbContext,
    HybridCache cache,
    ILogger<GetAllResumesCommandHandler> logger)
    : ICommandHandler<GetAllResumesCommand, ErrorOr<GetAllResumesResponse>>
{
    private const string HandlerName = nameof(GetAllResumesCommandHandler);

    public async Task<ErrorOr<GetAllResumesResponse>> ExecuteAsync(GetAllResumesCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {UserId}", HandlerName, command.UserId);

        var response = await cache.GetOrCreateAsync(
            ResumesCacheKeys.List(command.UserId),
            async cacheCt =>
            {
                var resumes = await dbContext.Resumes
                    .AsNoTracking()
                    .Include(r => r.Person)
                    .Include(r => r.Experiences.OrderBy(e => e.Order))
                    .Include(r => r.Educations.OrderBy(e => e.Order))
                    .Include(r => r.Skills.OrderBy(s => s.Order))
                    .Include(r => r.Languages.OrderBy(l => l.Order))
                    .Include(r => r.Courses.OrderBy(c => c.Order))
                    .Include(r => r.Projects.OrderBy(p => p.Order))
                    .Include(r => r.Links.OrderBy(l => l.Order))
                    .Where(r => r.UserId == command.UserId)
                    .OrderByDescending(r => r.CreatedAt)
                    .ToListAsync(cacheCt);

                logger.LogInformation(
                    "[SUCCESS] {HandlerName} | Found {Count} resumes in the database for {UserId}",
                    HandlerName,
                    resumes.Count,
                    command.UserId);

                return resumes.MapToResponse();
            },
            tags: [ResumesCacheKeys.UserResumesTag(command.UserId)],
            cancellationToken: ct);

        return response;
    }
}
