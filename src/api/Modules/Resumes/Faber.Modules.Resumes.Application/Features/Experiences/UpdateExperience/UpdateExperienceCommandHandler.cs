using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Experiences.UpdateExperience;

public class UpdateExperienceCommandHandler(
    ResumesDbContext dbContext,
    ILogger<UpdateExperienceCommandHandler> logger)
    : ICommandHandler<UpdateExperienceCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(UpdateExperienceCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(UpdateExperienceCommand command, CancellationToken ct)
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

        experience.JobTitle = command.JobTitle;
        experience.Employer = command.Employer;
        experience.StartDate = command.StartDate;
        experience.EndDate = command.EndDate;
        experience.City = command.City;
        experience.Description = HtmlContentSanitizer.SanitizeBasicFormatting(command.Description);

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Experience {ExperienceId} updated",
            HandlerName,
            command.Id);

        return true;
    }
}
