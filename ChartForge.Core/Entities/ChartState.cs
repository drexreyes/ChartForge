using ChartForge.Core.Entities;
using ChartForge.Core.Enums;

namespace ChartForge.Core.Entities;

public class ChartState
{
    public Guid Id { get; set; }

    /// <summary>
    /// Foreign key to the parent Conversation
    /// Every chart version belongs to exactly one conversation.
    /// </summary>
    public Guid ConversationId { get; set; }

    // Sequential version number within the conversation (1, 2, 3...)
    // This is scoped per-conversation, not globally unique.
    // Conversation A can have a Version 1, and so can Conversation B.
    // The unique constraint is on (ConversationId + VersionNumber) together
    // We will enforce that in the Infrastructure layer's Fluent API config
    public int VersionNumber { get; set; }

    // Strongly-typed enum telling us which JavaScript library to invoke
    // when rendering this chart version on the canvas
    public ChartLibrary ChartLibrary { get; set; }

    // This is the most important field in the entire entity
    // It is the raw JSON configuration object exactly as returned by n8n,
    // stored as a plain string. The Blazor server treats this as an
    // opaque blob - it never inspects or modifies it. It is passed directly to the JavScript
    // charting library via JS Interop.
    // Storing it as a string keeps our C# model completely decoupled from the ever-changing
    // structure of charting library configs.
    public string ChartConfigJson { get; set; } = string.Empty; // impedance mismatch under external dependency churn
    
    // Nullable because the n8n response may not always include metrics.
    // For example, a purely structural chart (org chart, flow diagram)
    // may have no meaningful summary statistics to display.
    public string? SummaryMetricsJson {  get; set; }

    // Nullable for the same reason - not every chart refinement will
    // produce contextually relevant quick-action suggestions.
    public string? SuggestedActionsJson { get; set; }

    // A human-readable description of what changed in this version,
    // e.g. "Brand colours + data labels". Nullable because the AI may not
    // always generate a meaningful label for every version.
    public string? VersionLabel { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    // --- Navigation Properties ---

    // Navigating "up" to the parent conversation.
    public Conversation Conversation { get; set; } = null!;

    // Navigating "across" to the specific AI message that triggered
    // the creation of this chart version. This is the reverse side of
    // the optional relationship we defined in Message.cs.
    // It is nullable because ChartState is created independently -
    // the Message references the ChartState, not the other way around.
    public Message? Message { get; set; }
}