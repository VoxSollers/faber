using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Skills.GetAllSkills;

public class GetAllSkillsCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetAllSkillsCommandHandler> logger)
    : ICommandHandler<GetAllSkillsCommand, ErrorOr<GetAllSkillsResponse>>
{
    private const string HandlerName = nameof(GetAllSkillsCommandHandler);

    public async Task<ErrorOr<GetAllSkillsResponse>> ExecuteAsync(GetAllSkillsCommand command, CancellationToken ct)
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

        var skills = await dbContext.Skills
            .AsNoTracking()
            .Where(s => s.ResumeId == command.ResumeId)
            .OrderBy(s => s.Order)
            .ToListAsync(ct);

        var items = skills.Select(s => s.MapToItem()).ToList();

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Found {Count} skills for resume {ResumeId}",
            HandlerName,
            items.Count,
            command.ResumeId);

        return new GetAllSkillsResponse(items);
    }
}