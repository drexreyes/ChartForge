using System.Text.RegularExpressions;

namespace ChartForge.Web.Services;

public static class MarkdownCodeExtractor
{
    private static readonly Regex CodeBlockRegex = new(
        @"```(?:javascript|js)?\s*\r?\n([\s\S]*?)\r?\n?\s*```",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    /// <summary>
    /// Extracts the JavaScript source from a markdown code block.
    /// Returns the original string trimmed if no code fence is present.
    /// </summary>
    public static string ExtractJavaScript(string markdown)
    {
        if (string.IsNullOrEmpty(markdown))
            return markdown;

        var match = CodeBlockRegex.Match(markdown);
        return match.Success ? match.Groups[1].Value.Trim() : markdown.Trim();
    }
}
