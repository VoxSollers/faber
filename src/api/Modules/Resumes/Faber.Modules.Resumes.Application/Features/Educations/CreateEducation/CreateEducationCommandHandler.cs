using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Educations.CreateEducation;

public class CreateEducationCommandHandler(
    ResumesDbContext dbContext,
    ILogger<CreateEducationCommandHandler> logger)
    : ICommandHandler<CreateEducationCommand, ErrorOr<CreateEducationResponse>>
{
    private const string HandlerName = nameof(CreateEducationCommandHandler);

    public async Task<ErrorOr<CreateEducationResponse>> ExecuteAsync(
        CreateEducationCommand command,
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

        var maxOrder = await dbContext.Educations
            .Where(e => e.ResumeId == command.ResumeId)
            .MaxAsync(e => (int?)e.Order, ct);

        var education = new Domain.Entities.Education
        {
            Id = Guid.NewGuid(),
            ResumeId = command.ResumeId,
            School = command.School,
            Degree = command.Degree,
            StartDate = command.StartDate,
            EndDate = command.EndDate,
            City = command.City,
            Description = HtmlContentSanitizer.SanitizeBasicFormatting(command.Description),
            Order = (maxOrder ?? -1) + 1
        };

        dbContext.Educations.Add(education);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Education {EducationId} created for resume {ResumeId}",
            HandlerName,
            education.Id,
            command.ResumeId);

        return education.MapToResponse();
    }
}