namespace Faber.Modules.Resumes.Application.Features.Resumes.Shared;

public record PersonResponse(
    Guid Id,
    string? JobTitle,
    string? Photo,
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
