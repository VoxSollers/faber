using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Educations.GetEducation;

public class GetEducationCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetEducationCommandHandler> logger)
    : ICommandHandler<GetEducationCommand, ErrorOr<GetEducationResponse>>
{
    private const string HandlerName = nameof(GetEducationCommandHandler);

    public async Task<ErrorOr<GetEducationResponse>> ExecuteAsync(GetEducationCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {EducationId}", HandlerName, command.Id);

        var education = await dbContext.Educations
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == command.Id && e.ResumeId == command.ResumeId, ct);

        if (education is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Education {EducationId} not found", HandlerName, command.Id);

            return Error.NotFound("Education.NotFound", $"Education with id '{command.Id}' was not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | Education {EducationId} found", HandlerName, command.Id);

        return education.MapToResponse();
    }
}