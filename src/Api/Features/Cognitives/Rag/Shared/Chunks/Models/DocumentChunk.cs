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

    [VectorStoreRecordData] public required string Category1 { get; init; }

    [VectorStoreRecordData] public required string Category2 { get; init; }

    [VectorStoreRecordData] public required decimal ScoreCategory1 { get; init; }

    [VectorStoreRecordData] public required decimal ScoreCategory2 { get; init; }
}