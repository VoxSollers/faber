using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Skills.GetSkill;

public class GetSkillCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetSkillCommandHandler> logger)
    : ICommandHandler<GetSkillCommand, ErrorOr<GetSkillResponse>>
{
    private const string HandlerName = nameof(GetSkillCommandHandler);

    public async Task<ErrorOr<GetSkillResponse>> ExecuteAsync(GetSkillCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {SkillId}", HandlerName, command.Id);

        var skill = await dbContext.Skills
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == command.Id && s.ResumeId == command.ResumeId, ct);

        if (skill is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Skill {SkillId} not found", HandlerName, command.Id);

            return Error.NotFound("Skill.NotFound", $"Skill with id '{command.Id}' was not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | Skill {SkillId} found", HandlerName, command.Id);

        return skill.MapToResponse();
    }
}