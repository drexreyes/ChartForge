using ChartForge.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChartForge.Infrastructure.Data.Configuration;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // --- Table ---
        // Explicitly name the table. Without this, EF Core defaults to the
        // DbSet property name ("Users") which happens to match - but being
        // explicit means a rename of the DbSet property never silently
        // renames our production table.
        builder.ToTable("Users");

        // --- Primary Key ---
        // EF Core can infer this from the "Id" naming convention, but
        // explicit configuration is always cleaner and more resilient.
        builder.HasKey(u => u.Id);

        // EF Core will use a database-generated GUID by default.
        // We override this to generate the GUID in the application, not the
        // database. This means we can know the Id before the INSERT completes,
        // which is important for setting up relationships in a single unit of work.
        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        

        // --- Properties ---
        builder.Property(u => u.SsoSubjectId)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.DisplayName)
            .IsRequired()
            .HasMaxLength(200);

        // nvarchar(320) matches the maximum theoretical length of an
        // email address as defined by RFC 5321
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);
        // No .IsRequired() here - nullable in C# maps to nullable in SQL,
        // but being explicit about max length is still good practice.

        builder.Property(u => u.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("datetime2"); // year 0001

        builder.Property(u => u.LastLoginAtUtc)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        // --- Indexes ---
        // This is the most important constraint on this table.
        // SsoSubjectId is our external identity key - the value from the
        // SSO provider that we use to look up returning users on every login.
        // IsUnique() enforces the business rule that one SSO identity maps
        // to exactly one application user, preventing duplicate provisioning.
        // The index dramatically speeds up that per-login lookup query.
        builder.HasIndex(u => u.SsoSubjectId)
            .IsUnique();

        // A non-unique index on Email for fast lookup by email if needed,
        // e.g. for admin search or support tooling in the future.
        builder.HasIndex(u => u.Email);

        // --- Relationships ---
        // User has many Conversations. If a User is deleted (hard delete),
        // restrict the operation rather than cascase - we use soft deletes,
        // so this is a safety net preventing accidental data destruction.
        builder.HasMany(u => u.Conversations)
            .WithOne(c => c.User)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}