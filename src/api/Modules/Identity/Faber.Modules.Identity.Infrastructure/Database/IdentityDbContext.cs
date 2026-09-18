using Faber.Modules.Identity.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using static Faber.Modules.Identity.Infrastructure.Database.DbConstants;

namespace Faber.Modules.Identity.Infrastructure.Database;

public class IdentityDbContext(DbContextOptions<IdentityDbContext> options) : DbContext(options)
{
    public DbSet<ActionToken> ActionTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaName);

        modelBuilder.Entity<ActionToken>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity
                .Property(x => x.Id)
                .ValueGeneratedNever()
                .HasConversion(
                    id => id.ToString(),
                    s => Ulid.Parse(s))
                .HasMaxLength(UlidMaxLength)
                .IsFixedLength();

            entity
                .Property(x => x.Selector)
                .ValueGeneratedNever()
                .HasConversion(
                    id => id.ToString(),
                    s => Ulid.Parse(s))
                .HasMaxLength(UlidMaxLength)
                .IsFixedLength();


            entity.HasIndex(x => x.Selector);
            entity.Property(x => x.Hash).IsRequired();
            entity.Property(x => x.Salt).IsRequired();
            entity.Property(x => x.ExpiresAt).IsRequired();
        });
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<Enum>()
            .HaveConversion<string>()
            .HaveColumnType($"varchar({TokenTypeMaxLength})");
    }
}
