using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Skills.DeleteSkill;

public class DeleteSkillCommandHandler(
    ResumesDbContext dbContext,
    ILogger<DeleteSkillCommandHandler> logger)
    : ICommandHandler<DeleteSkillCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(DeleteSkillCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(DeleteSkillCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {SkillId}", HandlerName, command.Id);

        var skill = await dbContext.Skills
            .FirstOrDefaultAsync(s => s.Id == command.Id && s.ResumeId == command.ResumeId, ct);

        if (skill is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Skill {SkillId} not found", HandlerName, command.Id);

            return Error.NotFound("Skill.NotFound", $"Skill with id '{command.Id}' was not found");
        }

        dbContext.Skills.Remove(skill);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Skill {SkillId} deleted", HandlerName, command.Id);

        return true;
    }
}