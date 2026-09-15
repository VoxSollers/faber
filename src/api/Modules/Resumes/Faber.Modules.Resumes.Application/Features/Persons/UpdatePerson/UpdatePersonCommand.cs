using ErrorOr;
using FastEndpoints;

namespace Faber.Modules.Resumes.Application.Features.Persons.UpdatePerson;

public record UpdatePersonCommand(
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
    string? DrivingLicense) : ICommand<ErrorOr<bool>>;