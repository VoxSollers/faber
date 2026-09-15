using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Experiences.GetAllExperiences;

public class GetAllExperiencesCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetAllExperiencesCommandHandler> logger)
    : ICommandHandler<GetAllExperiencesCommand, ErrorOr<GetAllExperiencesResponse>>
{
    private const string HandlerName = nameof(GetAllExperiencesCommandHandler);

    public async Task<ErrorOr<GetAllExperiencesResponse>> ExecuteAsync(
        GetAllExperiencesCommand command,
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

        var experiences = await dbContext.Experiences
            .AsNoTracking()
            .Where(e => e.ResumeId == command.ResumeId)
            .OrderBy(e => e.Order)
            .ToListAsync(ct);

        var items = experiences.Select(e => e.MapToItem()).ToList();

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Found {Count} experiences for resume {ResumeId}",
            HandlerName,
            items.Count,
            command.ResumeId);

        return new GetAllExperiencesResponse(items);
    }
}