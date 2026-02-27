namespace ChartForge.Core.Entities;
public class Conversation
{
    public Guid Id { get; set; }
    
    // This is the foreign key value - the raw Guid that links to the owner.
    // EF Core needs this explicit FK property for efficient querying.
    // Without it, EF uses a "shadow property" which is hidden and
    // harder to work with in queries and updates.
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;

    // Stores an icon identifier string (e.g., "bar", "line", "pie")
    // derived from the latest ChartState
    // This is nullable because a brand-new conversation has no chart yet.
    public string? ChartTypeIcon { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; } = false;

    // --- Navigation Properties ---

    // The parent - navigating "up" to the User who owns this conversation.
    public User User { get; set; } = null!;

    // The children - navigating "down" to all messages and chart versions
    // Initialized to empty lists for the same null-safety reason as in User.cs
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<ChartState> ChartStates { get; set; } = new List<ChartState>();
}