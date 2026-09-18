using Faber.Modules.Resumes.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using static Faber.Modules.Resumes.Infrastructure.Database.DbConstants;

namespace Faber.Modules.Resumes.Infrastructure.Database;

public class ResumesDbContext(DbContextOptions<ResumesDbContext> options) : DbContext(options)
{
    public DbSet<Resume> Resumes => Set<Resume>();

    public DbSet<Person> Persons => Set<Person>();

    public DbSet<Experience> Experiences => Set<Experience>();

    public DbSet<Education> Educations => Set<Education>();

    public DbSet<Course> Courses => Set<Course>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<Link> Links => Set<Link>();

    public DbSet<Skill> Skills => Set<Skill>();

    public DbSet<Language> Languages => Set<Language>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<Resume>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity
                .HasOne(x => x.Person)
                .WithOne(x => x.Resume)
                .HasForeignKey<Person>(x => x.ResumeId);

            entity
                .HasMany(x => x.Educations)
                .WithOne(x => x.Resume)
                .HasForeignKey(x => x.ResumeId);

            entity
                .HasMany(x => x.Experiences)
                .WithOne(x => x.Resume)
                .HasForeignKey(x => x.ResumeId);

            entity
                .HasMany(x => x.Languages)
                .WithOne(x => x.Resume)
                .HasForeignKey(x => x.ResumeId);

            entity
                .HasMany(x => x.Skills)
                .WithOne(x => x.Resume)
                .HasForeignKey(x => x.ResumeId);

            entity
                .HasMany(x => x.Links)
                .WithOne(x => x.Resume)
                .HasForeignKey(x => x.ResumeId);

            entity
                .HasMany(x => x.Courses)
                .WithOne(x => x.Resume)
                .HasForeignKey(x => x.ResumeId);

            entity
                .HasMany(x => x.Projects)
                .WithOne(x => x.Resume)
                .HasForeignKey(x => x.ResumeId);

            entity.Property(x => x.Title).HasMaxLength(OneLineStringMaxLength);

            entity
                .Property(x => x.Summary)
                .HasMaxLength(SummaryMaxLength);

            entity
                .Property(x => x.Localization)
                .HasMaxLength(OneLineStringMaxLength)
                .HasDefaultValue(DefaultLocalization);

            entity
                .Property(x => x.Hobbies)
                .HasMaxLength(HobbiesMaxLength);

            entity.Property(x => x.UserId).IsRequired();
            entity.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("personal_details");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.JobTitle).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Photo).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Firstname).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Lastname).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Email).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Phone).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Country).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.City).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Street).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.PostCode).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Nationality).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.DrivingLicense).HasMaxLength(OneLineStringMaxLength);
        });

        modelBuilder.Entity<Education>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Order).HasDefaultValue(0);
            entity.Property(x => x.School).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Degree).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.City).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Description).HasMaxLength(DescriptionMaxLength);
        });

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Order).HasDefaultValue(0);
            entity.Property(x => x.School).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Name).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Description).HasMaxLength(DescriptionMaxLength);
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Order).HasDefaultValue(0);
            entity.Property(x => x.Name).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Role).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Url).HasMaxLength(UriMaxLength);
            entity.Property(x => x.Description).HasMaxLength(DescriptionMaxLength);
        });

        modelBuilder.Entity<Experience>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Order).HasDefaultValue(0);
            entity.Property(x => x.JobTitle).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Employer).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.City).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Description).HasMaxLength(DescriptionMaxLength);
        });

        modelBuilder.Entity<Language>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Order).HasDefaultValue(0);
            entity.Property(x => x.Name).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Level).HasMaxLength(OneLineStringMaxLength);
        });

        modelBuilder.Entity<Skill>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Order).HasDefaultValue(0);
            entity.Property(x => x.Name).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Level).HasMaxLength(OneLineStringMaxLength);
        });

        modelBuilder.Entity<Link>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Order).HasDefaultValue(0);
            entity.Property(x => x.Label).HasMaxLength(OneLineStringMaxLength);
            entity.Property(x => x.Uri).HasMaxLength(UriMaxLength);
        });
    }
}
