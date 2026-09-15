using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Persons;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Persons.UpdatePerson;

public class UpdatePersonCommandHandler(
    ResumesDbContext dbContext,
    ILogger<UpdatePersonCommandHandler> logger)
    : ICommandHandler<UpdatePersonCommand, ErrorOr<bool>>
{
    private const string HandlerName = nameof(UpdatePersonCommandHandler);

    public async Task<ErrorOr<bool>> ExecuteAsync(UpdatePersonCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for {PersonId}", HandlerName, command.Id);

        var person = await dbContext.Persons
            .Include(p => p.Resume)
            .FirstOrDefaultAsync(p => p.Id == command.Id && p.ResumeId == command.ResumeId, ct);

        if (person is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Person {PersonId} not found", HandlerName, command.Id);

            return Error.NotFound("Person.NotFound", $"Person with id '{command.Id}' was not found");
        }

        person.JobTitle = command.JobTitle;
        person.Firstname = command.Firstname;
        person.Lastname = command.Lastname;
        person.Email = command.Email;
        person.Phone = command.Phone;
        person.Country = command.Country;
        person.City = command.City;
        person.Street = command.Street;
        person.PostCode = command.PostCode;
        person.Nationality = command.Nationality;
        person.DateOfBirth = command.DateOfBirth;
        person.DrivingLicense = command.DrivingLicense;

        if (person.Resume!.Title is null)
        {
            var title = ResumeTitleSnapshot.FromName(command.Firstname, command.Lastname);
            if (title is not null)
            {
                person.Resume.Title = title;
            }
        }

        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation("[SUCCESS] {HandlerName} | Person {PersonId} updated", HandlerName, command.Id);

        return true;
    }
}
