using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Experiences.CreateExperience;

public class CreateExperienceCommandHandler(
    ResumesDbContext dbContext,
    ILogger<CreateExperienceCommandHandler> logger)
    : ICommandHandler<CreateExperienceCommand, ErrorOr<CreateExperienceResponse>>
{
    private const string HandlerName = nameof(CreateExperienceCommandHandler);

    public async Task<ErrorOr<CreateExperienceResponse>> ExecuteAsync(
        CreateExperienceCommand command,
        CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for resume {ResumeId}", HandlerName, command.ResumeId);

        var resume = await dbContext.Resumes
            .FirstOrDefaultAsync(r => r.Id == command.ResumeId, ct);

        if (resume is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.ResumeId);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.ResumeId}' was not found");
        }

        var maxOrder = await dbContext.Experiences
            .Where(e => e.ResumeId == command.ResumeId)
            .MaxAsync(e => (int?)e.Order, ct);

        var experience = new Domain.Entities.Experience
        {
            Id = Guid.NewGuid(),
            ResumeId = command.ResumeId,
            JobTitle = command.JobTitle,
            Employer = command.Employer,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            City = command.City,
            Description = HtmlContentSanitizer.SanitizeBasicFormatting(command.Description),
            Order = (maxOrder ?? -1) + 1
        };

        dbContext.Experiences.Add(experience);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Experience {ExperienceId} created for resume {ResumeId}",
            HandlerName,
            experience.Id,
            command.ResumeId);

        return experience.MapToResponse();
    }
}