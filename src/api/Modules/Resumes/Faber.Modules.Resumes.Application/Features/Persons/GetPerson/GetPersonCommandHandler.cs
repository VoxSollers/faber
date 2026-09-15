using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Persons.GetPerson;

public class GetPersonCommandHandler(
    ResumesDbContext dbContext,
    ILogger<GetPersonCommandHandler> logger)
    : ICommandHandler<GetPersonCommand, ErrorOr<GetPersonResponse>>
{
    private const string HandlerName = nameof(GetPersonCommandHandler);

    public async Task<ErrorOr<GetPersonResponse>> ExecuteAsync(GetPersonCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {PersonId}", HandlerName, command.Id);

        var person = await dbContext.Persons
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == command.Id && p.ResumeId == command.ResumeId, ct);

        if (person is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Person {PersonId} not found", HandlerName, command.Id);

            return Error.NotFound("Person.NotFound", $"Person with id '{command.Id}' was not found");
        }

        logger.LogInformation("[SUCCESS] {HandlerName} | Person {PersonId} found", HandlerName, command.Id);

        return person.MapToResponse();
    }
}