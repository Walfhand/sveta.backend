using Api.Features.Cognitives.Rag.Shared.Chunks.Models;

namespace Api.Features.Cognitives.Rag.Shared.Abstractions;

public interface IRagWrite
{
    Task WriteAsync(string collectionName, IEnumerable<DocumentChunk> chunks, CancellationToken ct);
}