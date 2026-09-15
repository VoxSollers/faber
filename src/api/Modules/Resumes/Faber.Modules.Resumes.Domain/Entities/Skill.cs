namespace Faber.Modules.Resumes.Domain.Entities;

public class Skill : ResumeSection, IOrderable
{
    public string? Name { get; set; }

    public string? Level { get; set; }

    public int Order { get; set; }
}
