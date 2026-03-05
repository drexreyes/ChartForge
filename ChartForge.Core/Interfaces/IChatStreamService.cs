using ChartForge.Core.Models;
using System.Runtime.CompilerServices;

namespace ChartForge.Core.Interfaces;

public interface IChatStreamService
{
    public IAsyncEnumerable<StreamResult> StreamChatAsync(ChatRequest request, [EnumeratorCancellation] CancellationToken cancellationToken);
}