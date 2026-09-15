using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Skills.CreateSkill;

public class CreateSkillCommandHandler(
    ResumesDbContext dbContext,
    ILogger<CreateSkillCommandHandler> logger)
    : ICommandHandler<CreateSkillCommand, ErrorOr<CreateSkillResponse>>
{
    private const string HandlerName = nameof(CreateSkillCommandHandler);

    public async Task<ErrorOr<CreateSkillResponse>> ExecuteAsync(CreateSkillCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for resume {ResumeId}", HandlerName, command.ResumeId);

        var resume = await dbContext.Resumes
            .FirstOrDefaultAsync(r => r.Id == command.ResumeId, ct);

        if (resume is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.ResumeId);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.ResumeId}' was not found");
        }

        var maxOrder = await dbContext.Skills
            .Where(s => s.ResumeId == command.ResumeId)
            .MaxAsync(s => (int?)s.Order, ct);

        var skill = new Domain.Entities.Skill
        {
            Id = Guid.NewGuid(),
            ResumeId = command.ResumeId,
            Name = command.Name,
            Level = command.Level,
            Order = (maxOrder ?? -1) + 1
        };

        dbContext.Skills.Add(skill);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Skill {SkillId} created for resume {ResumeId}",
            HandlerName,
            skill.Id,
            command.ResumeId);

        return skill.MapToResponse();
    }
}