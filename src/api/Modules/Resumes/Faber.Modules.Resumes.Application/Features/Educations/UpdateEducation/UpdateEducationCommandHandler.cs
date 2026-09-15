using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Resumes.Shared;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Educations.UpdateEducation;

public class UpdateEducationCommandHandler(
    ResumesDbContext dbContext,
    ILogger<UpdateEducationCommandHandler> logger)
    : ICommandHandler<UpdateEducationCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(UpdateEducationCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(UpdateEducationCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {EducationId}", HandlerName, command.Id);

        var education = await dbContext.Educations
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.ResumeId == command.ResumeId, ct);

        if (education is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Education {EducationId} not found", HandlerName, command.Id);

            return Error.NotFound("Education.NotFound", $"Education with id '{command.Id}' was not found");
        }

        education.School = command.School;
        education.Degree = command.Degree;
        education.StartDate = command.StartDate;
        education.EndDate = command.EndDate;
        education.City = command.City;
        education.Description = HtmlContentSanitizer.SanitizeBasicFormatting(command.Description);

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Education {EducationId} updated", HandlerName, command.Id);

        return true;
    }
}
