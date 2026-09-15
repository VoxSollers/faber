namespace Faber.Modules.Resumes.Application.Features.Persons.CreatePerson;

public static class CreatePersonMapper
{
    public static CreatePersonCommand MapToCommand(this CreatePersonRequest request)
    {
        return new CreatePersonCommand(
            request.ResumeId,
            request.JobTitle,
            request.Firstname,
            request.Lastname,
            request.Email,
            request.Phone,
            request.Country,
            request.City,
            request.Street,
            request.PostCode,
            request.Nationality,
            request.DateOfBirth,
            request.DrivingLicense);
    }

    public static CreatePersonResponse MapToResponse(this Domain.Entities.Person person)
    {
        return new CreatePersonResponse(
            person.Id,
            person.ResumeId,
            person.JobTitle,
            person.Firstname,
            person.Lastname,
            person.Email,
            person.Phone,
            person.Country,
            person.City,
            person.Street,
            person.PostCode,
            person.Nationality,
            person.DateOfBirth,
            person.DrivingLicense);
    }
}