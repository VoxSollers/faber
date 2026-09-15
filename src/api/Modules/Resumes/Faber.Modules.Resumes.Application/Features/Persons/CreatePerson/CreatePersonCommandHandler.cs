using ErrorOr;
using Faber.Modules.Resumes.Application.Features.Persons;
using Faber.Modules.Resumes.Infrastructure.Database;
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Faber.Modules.Resumes.Application.Features.Persons.CreatePerson;

public class CreatePersonCommandHandler(
    ResumesDbContext dbContext,
    ILogger<CreatePersonCommandHandler> logger)
    : ICommandHandler<CreatePersonCommand, ErrorOr<CreatePersonResponse>>
{
    private const string HandlerName = nameof(CreatePersonCommandHandler);

    public async Task<ErrorOr<CreatePersonResponse>> ExecuteAsync(CreatePersonCommand command, CancellationToken ct)
    {
        logger.LogInformation("[START] {HandlerName} for resume {ResumeId}", HandlerName, command.ResumeId);

        var resume = await dbContext.Resumes
            .FirstOrDefaultAsync(r => r.Id == command.ResumeId, ct);

        if (resume is null)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Resume {ResumeId} not found", HandlerName, command.ResumeId);

            return Error.NotFound("Resume.NotFound", $"Resume with id '{command.ResumeId}' was not found");
        }

        var personExists = await dbContext.Persons
            .AnyAsync(p => p.ResumeId == command.ResumeId, ct);

        if (personExists)
        {
            logger.LogWarning("[FAIL] {HandlerName} | Person already exists for resume {ResumeId}", HandlerName, command.ResumeId);

            return Error.Conflict("Person.Conflict", "A person profile already exists for this resume.");
        }

        var person = new Domain.Entities.Person
        {
            Id = Guid.NewGuid(),
            ResumeId = command.ResumeId,
            JobTitle = command.JobTitle,
            Firstname = command.Firstname,
            Lastname = command.Lastname,
            Email = command.Email,
            Phone = command.Phone,
            Country = command.Country,
            City = command.City,
            Street = command.Street,
            PostCode = command.PostCode,
            Nationality = command.Nationality,
            DateOfBirth = command.DateOfBirth,
            DrivingLicense = command.DrivingLicense
        };

        if (resume.Title is null)
        {
            var title = ResumeTitleSnapshot.FromName(command.Firstname, command.Lastname);
            if (title is not null)
            {
                resume.Title = title;
            }
        }

        dbContext.Persons.Add(person);
        await dbContext.SaveChangesAsync(ct);

        logger.LogInformation(
            "[SUCCESS] {HandlerName} | Person {PersonId} created for resume {ResumeId}",
            HandlerName,
            person.Id,
            command.ResumeId);

        return person.MapToResponse();
    }
}
