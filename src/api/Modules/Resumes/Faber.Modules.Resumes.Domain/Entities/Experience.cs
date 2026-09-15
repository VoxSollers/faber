namespace Faber.Modules.Resumes.Domain.Entities;

public class Experience : ResumeSection, IOrderable
{
    public string? JobTitle { get; set; }

    public string? Employer { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? City { get; set; }

    public string? Description { get; set; }

    public int Order { get; set; }
}
