namespace Faber.Modules.Resumes.Application.Features.Persons.DeletePerson;

public record DeletePersonRequest(Guid ResumeId, Guid Id);