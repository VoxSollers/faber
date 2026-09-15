namespace Faber.Modules.Resumes.Application.Features.Persons.GetPersonByResume;

public static class GetPersonByResumeMapper
{
    public static GetPersonByResumeCommand MapToCommand(this GetPersonByResumeRequest request)
    {
        return new GetPersonByResumeCommand(request.ResumeId);
    }

    public static GetPersonByResumeResponse MapToResponse(this Domain.Entities.Person person)
    {
        return new GetPersonByResumeResponse(
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