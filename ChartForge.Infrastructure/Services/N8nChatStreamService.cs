using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using ChartForge.Core.Interfaces;
using ChartForge.Core.Models;

namespace ChartForge.Infrastructure.Services;

public class N8nChatStreamService : IChatStreamService
{
    private readonly HttpClient _httpClient;

    public N8nChatStreamService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async IAsyncEnumerable<StreamResult> StreamChatAsync(
        ChatRequest chatRequest,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var payload = new
        {
            chatInput = chatRequest.UserPrompt,
            currentCode = chatRequest.CurrentChartCode,
            chatHistory = chatRequest.History,
            dataSchema = ""
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "") { Content = content };
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("text/event-stream"));
        request.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

        var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var state = new StreamParseState();
        var jsonBuffer = new StringBuilder();

        while (!reader.EndOfStream && !cancellationToken.IsCancellationRequested)
        {
            var line = await reader.ReadLineAsync(cancellationToken);

            // Blank line = SSE event boundary; attempt to flush accumulated buffer.
            if (string.IsNullOrWhiteSpace(line))
            {
                if (jsonBuffer.Length > 0)
                {
                    var buffered = jsonBuffer.ToString();
                    jsonBuffer.Clear();

                    foreach (var result in ParseAndProcess(buffered, state))
                        yield return result;
                }
                continue;
            }

            string chunk = line.StartsWith("data: ", StringComparison.Ordinal)
                ? line["data: ".Length..]
                : line;

            if (chunk == "[DONE]")
                break;

            Console.WriteLine("RAW: " + chunk);

            if (TryParseJsonDocument(chunk, out var doc))
            {
                using (doc)
                    foreach (var result in ProcessDocument(doc!.RootElement, state))
                        yield return result;
            }
            else
            {
                jsonBuffer.Append(chunk);

                if (TryParseJsonDocument(jsonBuffer.ToString(), out var bufferedDoc))
                {
                    jsonBuffer.Clear();
                    using (bufferedDoc)
                        foreach (var result in ProcessDocument(bufferedDoc!.RootElement, state))
                            yield return result;
                }
            }
        }

        if (state.ChartCodeBuilder.Length > 0)
            yield return new StreamResult { FinalChartCode = state.ChartCodeBuilder.ToString() };
    }


    private static bool TryParseJsonDocument(string text, out JsonDocument? doc)
    {
        try
        {
            doc = JsonDocument.Parse(text);
            return true;
        }
        catch (JsonException)
        {
            doc = null;
            return false;
        }
    }

    private static IEnumerable<StreamResult> ParseAndProcess(string text, StreamParseState state)
    {
        if (!TryParseJsonDocument(text, out var doc))
            yield break;

        using (doc)
            foreach (var result in ProcessDocument(doc!.RootElement, state))
                yield return result;
    }

    private static IEnumerable<StreamResult> ProcessDocument(JsonElement root, StreamParseState state)
    {
        IEnumerable<JsonElement> elements = root.ValueKind switch
        {
            JsonValueKind.Array => root.EnumerateArray(),
            JsonValueKind.Object => new[] { root },
            _ => Enumerable.Empty<JsonElement>()
        };

        foreach (var element in elements)
        {
            if (!element.TryGetProperty("type", out var typeProp))
                continue;

            var type = typeProp.GetString();

            if (!element.TryGetProperty("metadata", out var metadata))
                continue;

            if (!metadata.TryGetProperty("nodeName", out var nodeNameProp))
                continue;

            var nodeName = nodeNameProp.GetString();

            if (type == "begin" && nodeName is not null)
                state.ActiveNode = MapNodeName(nodeName);
            else if (type == "end")
                state.ActiveNode = WorkflowNodeType.Unknown;

            if (state.ActiveNode == WorkflowNodeType.Unknown)
                continue;

            if (type == "item" && element.TryGetProperty("content", out var contentProp))
            {
                var contentStr = contentProp.GetString();
                if (string.IsNullOrEmpty(contentStr))
                    continue;

                if (state.ActiveNode == WorkflowNodeType.MainAgent)
                {
                    if (contentStr.Contains("```"))
                    {
                        state.IsInsideCodeBlock = !state.IsInsideCodeBlock;
                        continue;
                    }

                    if (!state.IsInsideCodeBlock)
                        yield return new StreamResult { AssistantChunk = contentStr };
                }
                else if (state.ActiveNode == WorkflowNodeType.ChartAgent)
                {
                    state.ChartCodeBuilder.Append(contentStr);
                }
            }
        }
    }

    private static WorkflowNodeType MapNodeName(string nodeName) =>
        nodeName switch
        {
            string n when n.Contains("Main Agent", StringComparison.OrdinalIgnoreCase) => WorkflowNodeType.MainAgent,
            string n when n.Contains("Output", StringComparison.OrdinalIgnoreCase)     => WorkflowNodeType.OutputNode,
            string n when n.Contains("Charts.js", StringComparison.OrdinalIgnoreCase)  => WorkflowNodeType.ChartAgent,
            string n when n.Contains("chartjs", StringComparison.OrdinalIgnoreCase)    => WorkflowNodeType.ChartAgent,
            string n when n.Contains("chartsjs", StringComparison.OrdinalIgnoreCase)   => WorkflowNodeType.ChartAgent,
            string n when n.Contains("chart.js", StringComparison.OrdinalIgnoreCase)   => WorkflowNodeType.ChartAgent,
            string n when n.Contains("ECharts", StringComparison.OrdinalIgnoreCase)    => WorkflowNodeType.ChartAgent,
            string n when n.Contains("D3", StringComparison.OrdinalIgnoreCase)         => WorkflowNodeType.ChartAgent,
            string n when n.Contains("Highcharts", StringComparison.OrdinalIgnoreCase) => WorkflowNodeType.ChartAgent,
            _ => WorkflowNodeType.Unknown
        };


    private sealed class StreamParseState
    {
        public WorkflowNodeType ActiveNode { get; set; } = WorkflowNodeType.Unknown;
        public bool IsInsideCodeBlock { get; set; }
        public StringBuilder ChartCodeBuilder { get; } = new();
    }
}

public enum WorkflowNodeType
{
    Unknown,
    MainAgent,
    ChartAgent,
    OutputNode,
}
