using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Educations.GetAllEducations;

public class GetAllEducationsCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetAllEducationsCommandHandler> logger)
    : ICommandHandler<GetAllEducationsCommand, ErrorOr<GetAllEducationsResponse>>
{
    private const string HandlerName = nameof(GetAllEducationsCommandHandler);

    public async Task<ErrorOr<GetAllEducationsResponse>> ExecuteAsync(
        GetAllEducationsCommand command,
        CancellationToken ct)
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

        var educations = await dbContext.Educations
            .AsNoTracking()
            .Where(e => e.ResumeId == command.ResumeId)
            .OrderBy(e => e.Order)
            .ToListAsync(ct);

        var items = educations.Select(e => e.MapToItem()).ToList();

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Found {Count} educations for resume {ResumeId}",
            HandlerName,
            items.Count,
            command.ResumeId);

        return new GetAllEducationsResponse(items);
    }
}