using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Links.GetAllLinks;

public class GetAllLinksCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetAllLinksCommandHandler> logger)
    : ICommandHandler<GetAllLinksCommand, ErrorOr<GetAllLinksResponse>>
{
    private const string HandlerName = nameof(GetAllLinksCommandHandler);

    public async Task<ErrorOr<GetAllLinksResponse>> ExecuteAsync(GetAllLinksCommand command, CancellationToken ct)
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

        var links = await dbContext.Links
            .AsNoTracking()
            .Where(e => e.ResumeId == command.ResumeId)
            .OrderBy(e => e.Order)
            .ToListAsync(ct);

        var items = links.Select(e => e.MapToItem()).ToList();

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Found {Count} links for resume {ResumeId}",
            HandlerName,
            items.Count,
            command.ResumeId);

        return new GetAllLinksResponse(items);
    }
}