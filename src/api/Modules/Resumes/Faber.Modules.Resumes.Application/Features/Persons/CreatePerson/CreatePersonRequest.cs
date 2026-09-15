namespace Faber.Modules.Resumes.Application.Features.Persons.CreatePerson;

public record CreatePersonRequest(
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