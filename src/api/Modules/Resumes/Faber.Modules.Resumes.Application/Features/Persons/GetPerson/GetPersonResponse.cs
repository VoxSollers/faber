namespace Faber.Modules.Resumes.Application.Features.Persons.GetPerson;

public record GetPersonResponse(
    Guid Id,
    Guid ResumeId,
    string? JobTitle,
    string? Firstname,
    string? Lastname,
    string? Email,
    string? Phone,
    string? Country,
    string? City,
    string? Street,
    string? PostCode,
    string? Nationality,
    DateOnly? DateOfBirth,
    string? DrivingLicense);