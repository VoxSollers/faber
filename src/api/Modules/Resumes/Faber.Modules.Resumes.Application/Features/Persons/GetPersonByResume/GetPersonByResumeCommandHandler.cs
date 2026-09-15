using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Persons.GetPersonByResume;

public class GetPersonByResumeCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetPersonByResumeCommandHandler> logger)
    : ICommandHandler<GetPersonByResumeCommand, ErrorOr<GetPersonByResumeResponse>>
{
    private const string HandlerName = nameof(GetPersonByResumeCommandHandler);

    public async Task<ErrorOr<GetPersonByResumeResponse>> ExecuteAsync(
        GetPersonByResumeCommand command,
        CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for resume {ResumeId}", HandlerName, command.ResumeId);

        var person = await dbContext.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ResumeId == command.ResumeId, ct);

        if (person is null)
        {
            logger.LogWarning(
                "[FAIL] {HandlerName} | Person not found for resume {ResumeId}",
                HandlerName,
                command.ResumeId);

            return Error.NotFound("Person.NotFound", $"Person for resume '{command.ResumeId}' was not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | Person {PersonId} found", HandlerName, person.Id);

        return person.MapToResponse();
    }
}