using ChartForge.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChartForge.Infrastructure.Data.Configuration;

public class ConversationConfiguration : IEntityTypeConfiguration<Conversation>
{
    public void Configure(EntityTypeBuilder<Conversation> builder)
    {
        // --- Table ---
        builder.ToTable("Conversations");

        // --- Primary Key ---
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .ValueGeneratedNever();


        // --- Properties ---
        // Auto-generated from the first user message.
        // This will be populated bago i-save sa database
        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(300);

        // Nullable ito kasi kapag bago pa lang 'yung conversation
        // wala pa siyang chart, or may mga convo na nagtatanong lang si user
        // amnd wala talagang generated na chart
        builder.Property(c => c.ChartTypeIcon)
            .HasMaxLength(50);

        builder.Property(c => c.CreatedAtUtc)
            .IsRequired()
            .HasColumnType("datetime2");

        // ito 'yung gagamitin for the sidebar grouping (today, earlier, yesterday, etc)
        builder.Property(c => c.UpdatedAtUtc)
            .IsRequired()
            .HasColumnType("datetime2");

        builder.Property(c => c.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // --- Indexes ---
        /*
         * SELECT * FROM Conversations
         * WHERE UserId = @currentUserId
         * AND IsDelete = 0
         * ORDER BY UpdatedAtUtc DESC
         */
        builder.HasIndex(c => new { c.UserId, c.UpdatedAtUtc }) // Composite Index
            .HasFilter("[IsDeleted] = 0"); // Filtered Index

        // --- Relationships

        // A conversation belongs to isang User.
        builder.HasOne(c => c.User)
            .WithMany(u => u.Conversations)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // A conversation can have many messages
        builder.HasMany(c => c.Messages)
            .WithOne(m => m.Conversation)
            .HasForeignKey(m => m.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);

        // Conversation has many ChartStates
        builder.HasMany(c => c.ChartStates)
            .WithOne(cs => cs.Conversation)
            .HasForeignKey(cs => cs.ConversationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}