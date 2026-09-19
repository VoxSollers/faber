namespace Faber.Modules.Resumes.Application.Features.Resumes.Shared;

public record ResumeResponse(
    Guid Id,
    DateTimeOffset CreatedAt,
    string? Title,
    string? Localization,
    string? Summary,
    string? Hobbies,
    PersonResponse? Person,
    List<ExperienceResponse> Experience,
    List<EducationResponse> Educations,
    List<SkillResponse> Skills,
    List<LanguageResponse> Languages,
    List<CourseResponse> Courses,
    List<ProjectResponse> Projects,
    List<LinkResponse> Links);
