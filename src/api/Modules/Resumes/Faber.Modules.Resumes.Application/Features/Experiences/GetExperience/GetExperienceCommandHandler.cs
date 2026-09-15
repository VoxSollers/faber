using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Experiences.GetExperience;

public class GetExperienceCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetExperienceCommandHandler> logger)
    : ICommandHandler<GetExperienceCommand, ErrorOr<GetExperienceResponse>>
{
    private const string HandlerName = nameof(GetExperienceCommandHandler);

    public async Task<ErrorOr<GetExperienceResponse>> ExecuteAsync(
        GetExperienceCommand command,
        CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {ExperienceId}", HandlerName, command.Id);

        var experience = await dbContext.Experiences
            .AsNoTracking()
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

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Experience {ExperienceId} found",
            HandlerName,
            command.Id);

        return experience.MapToResponse();
    }
}