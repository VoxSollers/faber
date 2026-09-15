namespace Faber.Modules.Resumes.Application.Features.Persons.UpdatePerson;

public static class UpdatePersonMapper
{
    public static UpdatePersonCommand MapToCommand(this UpdatePersonRequest request)
    {
        return new UpdatePersonCommand(
            request.Id,
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
}