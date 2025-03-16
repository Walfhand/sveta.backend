using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;

namespace Api.Features.Cognitives.Rag.Shared.Chunks.Models;
#pragma warning disable SKEXP0001
public class DocumentChunk
{
    [VectorStoreRecordKey] public required string Key { get; init; }

    [TextSearchResultName]
    [VectorStoreRecordData]
    public required string DocumentName { get; init; }

    [TextSearchResultLink]
    [VectorStoreRecordData]
    public required int ChunkNumber { get; init; }

    [VectorStoreRecordData]
    [TextSearchResultValue]
    public required string Text { get; init; }

    [VectorStoreRecordVector(1024)] public ReadOnlyMemory<float> TextEmbedding { get; set; }
}