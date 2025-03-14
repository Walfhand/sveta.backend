using Api.Features.Projects.Features.Documents.ChunkDocument.Models;

namespace Api.Rag.Abstractions;

public interface IRagWrite
{
    Task WriteAsync(string collectionName, IEnumerable<DocumentChunk> chunks, CancellationToken ct);
}