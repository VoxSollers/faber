using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Projects.GetAllProjects;

public class GetAllProjectsCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetAllProjectsCommandHandler> logger)
    : ICommandHandler<GetAllProjectsCommand, ErrorOr<GetAllProjectsResponse>>
{
    private const string HandlerName = nameof(GetAllProjectsCommandHandler);

    public async Task<ErrorOr<GetAllProjectsResponse>> ExecuteAsync(GetAllProjectsCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for resume {ResumeId}", HandlerName, command.ResumeId);

        var resume = await dbContext.Resumes
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == command.ResumeId, ct);

        if (resume is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.ResumeId);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.ResumeId}' was not found");
        }

        var projects = await dbContext.Projects
            .AsNoTracking()
            .Where(c => c.ResumeId == command.ResumeId)
            .OrderBy(c => c.Order)
            .ToListAsync(ct);

        var items = projects.Select(e => e.MapToItem()).ToList();

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Found {Count} projects for resume {ResumeId}",
            HandlerName,
            items.Count,
            command.ResumeId);

        return new GetAllProjectsResponse(items);
    }
}