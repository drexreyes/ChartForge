using ChartForge.Core.Enums;
using ChatForge.Core.Entities;

namespace ChartForge.Core.Entities;

public class Message
{
    public Guid Id { get; set; }

    // Foreign key to the parent Conversation
    public Guid ConversationId { get; set; }

    // Strongly-typed role using our enum. EF Coer will store this as a string
    // in the database (we configure that in the Infrastracture layer)
    // But in C# code we always work with the safe enum value
    // Never a raw string
    public MessageRole Role { get; set; }

    // The full content of the message, from either the user or the AI.
    // Note: chart configuration data is NOT stored here. It lives in ChartState.
    // This column holds only human-readable text.
    public string Content { get; set; } = string.Empty;

    // This is the optional link between an AI message and the chart version it produced.
    // It is nullable because:

    // 1. All user messages have no associated chart (they are prompts, not response).
    // 2. An AI message might be a text-only clarification with no new chart generated.
    
    // Only AI messages that trigger a new chart version will have this populated.
    public Guid? ChartStateId { get; set; }
    public DateTime SentAtUtc { get; set; }

    // This integer ensures we can always retrieve and display messages in the correct order, regardless of any timestamp collisions.
    // Two messages in the same millisecond is unlikely but not impossible.
    // It is the authoritative ordering mechanism - never rely on timestamps alone.
    public int SequenceNumber { get; set; }

    // --- Navigation Properties ---

    // Navigating "up" to the parent conversation.
    public Conversation Conversation { get; set; } = null!;

    // Navigating "across" to the chart state this message produced.
    // This is nullable to match the nullable FK above.
    // A null here simply means: this message did not generate a chart version.
    public ChartState? ChartState { get; set; }
}