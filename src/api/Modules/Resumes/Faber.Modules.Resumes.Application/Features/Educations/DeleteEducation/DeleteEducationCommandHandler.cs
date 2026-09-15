using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Educations.DeleteEducation;

public class DeleteEducationCommandHandler(
    ResumesDbContext dbContext,
    ILogger<DeleteEducationCommandHandler> logger)
    : ICommandHandler<DeleteEducationCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(DeleteEducationCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(DeleteEducationCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {EducationId}", HandlerName, command.Id);

        var education = await dbContext.Educations
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.ResumeId == command.ResumeId, ct);

        if (education is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Education {EducationId} not found", HandlerName, command.Id);

            return Error.NotFound("Education.NotFound", $"Education with id '{command.Id}' was not found");
        }

        dbContext.Educations.Remove(education);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Education {EducationId} deleted", HandlerName, command.Id);

        return true;
    }
}