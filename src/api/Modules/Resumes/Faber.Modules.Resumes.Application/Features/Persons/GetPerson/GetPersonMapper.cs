namespace Faber.Modules.Resumes.Application.Features.Persons.GetPerson;

public static class GetPersonMapper
{
    public static GetPersonCommand MapToCommand(this GetPersonRequest request)
    {
        return new GetPersonCommand(request.ResumeId, request.Id);
    }

    public static GetPersonResponse MapToResponse(this Domain.Entities.Person person)
    {
        return new GetPersonResponse(
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