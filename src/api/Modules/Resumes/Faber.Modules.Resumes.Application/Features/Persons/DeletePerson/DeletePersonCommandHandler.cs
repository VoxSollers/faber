using ErrorOr;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Persons.DeletePerson;

public class DeletePersonCommandHandler(
    ResumesDbContext dbContext,
    ILogger<DeletePersonCommandHandler> logger)
    : ICommandHandler<DeletePersonCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(DeletePersonCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(DeletePersonCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {PersonId}", HandlerName, command.Id);

        var person = await dbContext.Persons
            .FirstOrDefaultAsync(p => p.Id == command.Id && p.ResumeId == command.ResumeId, ct);

        if (person is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Person {PersonId} not found", HandlerName, command.Id);

            return Error.NotFound("Person.NotFound", $"Person with id '{command.Id}' was not found");
        }

        dbContext.Persons.Remove(person);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Person {PersonId} deleted", HandlerName, command.Id);

        return true;
    }
}