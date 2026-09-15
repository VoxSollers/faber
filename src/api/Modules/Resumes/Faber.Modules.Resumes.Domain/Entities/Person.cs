namespace Faber.Modules.Resumes.Domain.Entities;

public class Person : ResumeSection
{
    public string? JobTitle { get; set; }

    public string? Photo { get; set; }

    public string? Firstname { get; set; }

    public string? Lastname { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Country { get; set; }

    public string? City { get; set; }

    public string? Street { get; set; }

    public string? PostCode { get; set; }

    public string? Nationality { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? DrivingLicense { get; set; }
}