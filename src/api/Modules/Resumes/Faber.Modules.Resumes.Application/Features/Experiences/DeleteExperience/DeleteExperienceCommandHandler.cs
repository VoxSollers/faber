using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Experiences.DeleteExperience;

public class DeleteExperienceCommandHandler(
    ResumesDbContext dbContext,
    ILogger<DeleteExperienceCommandHandler> logger)
    : ICommandHandler<DeleteExperienceCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(DeleteExperienceCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(DeleteExperienceCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {ExperienceId}", HandlerName, command.Id);

        var experience = await dbContext.Experiences
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.ResumeId == command.ResumeId, ct);

        if (experience is null)
        {
            logger.LogWarning(
                "[FAIL] {HandlerName} | Experience {ExperienceId} not found",
                HandlerName,
                command.Id);

            return Error.NotFound(
                "Experience.NotFound",
                $"Experience with id '{command.Id}' was not found");
        }

        dbContext.Experiences.Remove(experience);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Experience {ExperienceId} deleted",
            HandlerName,
            command.Id);

        return true;
    }
}