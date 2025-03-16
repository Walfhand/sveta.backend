using Api.Features.Rag.Chunks.Models;

namespace Api.Shared.Rag.Abstractions;

public interface IRagWrite
{
    Task WriteAsync(string collectionName, IEnumerable<DocumentChunk> chunks, CancellationToken ct);
}