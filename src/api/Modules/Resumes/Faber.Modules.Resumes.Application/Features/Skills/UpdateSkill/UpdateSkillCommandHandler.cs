using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Skills.UpdateSkill;

public class UpdateSkillCommandHandler(
    ResumesDbContext dbContext,
    ILogger<UpdateSkillCommandHandler> logger)
    : ICommandHandler<UpdateSkillCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(UpdateSkillCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(UpdateSkillCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {SkillId}", HandlerName, command.Id);

        var skill = await dbContext.Skills
            .FirstOrDefaultAsync(s => s.Id == command.Id && s.ResumeId == command.ResumeId, ct);

        if (skill is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Skill {SkillId} not found", HandlerName, command.Id);

            return Error.NotFound("Skill.NotFound", $"Skill with id '{command.Id}' was not found");
        }

        skill.Name = command.Name;
        skill.Level = command.Level;

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Skill {SkillId} updated", HandlerName, command.Id);

        return true;
    }
}
