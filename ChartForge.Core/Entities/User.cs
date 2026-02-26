namespace ChartForge.Core.Entities;

public class User
{
    public Guid Id { get; set; }

    // This is the unique identifier from the SSO provider.
    // It is our primary key for matching returning users, not the email address
    // Because emails can change but an SSO subject ID is immutable.
    public string SsoSubjectId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    // Nullable because the SSO provider may not always supply a profile image
    public string? AvatarUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime LastLoginAtUtc { get; set; }

    // Soft-delete flag. We never hard delete users to preserve audit trails
    // and referential integrity with their conversations.
    public bool IsActive { get; set; } = true;

    // --- Navigation Properties ----
    // EF Core uses these to understand the relationship between entities.
    // Notice we initialize the collection to an empty list - this is a best
    // practice that prevents a NullReferenceException if you access
    // .Conversations before EF Core has populated it.
    public ICollection<Conversation> Conversations { get; set; } = new List<Conversation>();
}