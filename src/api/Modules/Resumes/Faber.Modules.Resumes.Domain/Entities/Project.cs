namespace Faber.Modules.Resumes.Domain.Entities;

public class Project : ResumeSection, IOrderable
{
    public string? Name { get; set; }
    public string? Role { get; set; }
    public string? Url { get; set; }
    public DateOnly? StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string? Description { get; set; }
    public int Order { get; set; }
}
