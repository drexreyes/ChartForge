namespace ChartForge.Core.Enums;

/// <summary>
/// Identifies the sender of a message within a conversation.
/// Stored as a string in the database for readability in raw queries.
/// </summary>
public enum MessageRole
{
    /// <summary>
    /// The human user who sent the prompt.
    /// </summary>
    User,
    /// <summary>
    /// The AI assistant that generated the response.
    /// </summary>
    Assistant
}