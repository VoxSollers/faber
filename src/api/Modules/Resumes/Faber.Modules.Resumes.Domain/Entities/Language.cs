namespace Faber.Modules.Resumes.Domain.Entities;

public class Language : ResumeSection, IOrderable
{
    public string? Name { get; set; }

    public string? Level { get; set; }

    public int Order { get; set; }
}
