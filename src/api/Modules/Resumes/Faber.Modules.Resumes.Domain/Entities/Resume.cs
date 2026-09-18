namespace Faber.Modules.Resumes.Domain.Entities;

public class Resume
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Person? Person { get; set; }

    public string? Title { get; set; }

    public string? Summary { get; set; }

    public string? Localization { get; set; }

    public string? Hobbies { get; set; }

    public List<Experience> Experiences { get; set; } = [];

    public List<Education> Educations { get; set; } = [];

    public List<Link> Links { get; set; } = [];

    public List<Skill> Skills { get; set; } = [];

    public List<Language> Languages { get; set; } = [];

    public List<Course> Courses { get; set; } = [];

    public List<Project> Projects { get; set; } = [];
}
