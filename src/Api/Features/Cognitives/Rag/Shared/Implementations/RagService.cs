using Api.Features.Cognitives.Rag.Search;
using Api.Features.Cognitives.Rag.Shared.Abstractions;
using Api.Features.Cognitives.Rag.Shared.Chunks.Models;
using Api.Features.Projects.Domain;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace Api.Features.Cognitives.Rag.Shared.Implementations;
#pragma warning disable SKEXP0001
public class RagService(
    IVectorStore vectorStore,
    VectorSearchService vectorSearchService,
    ITextEmbeddingGenerationService embeddingGenerationService)
    : IRagRead, IRagWrite
{
    public async Task<List<ChunkResult>> ReadAsync(ProjectId projectId, string question, CancellationToken ct,
        VectorSearchOptions? options)
    {
        var collection = vectorStore.GetCollection<string, DocumentChunk>(projectId.Value.ToString());
        await collection.CreateCollectionIfNotExistsAsync(ct);

        var inputEmbedding = await embeddingGenerationService.GenerateEmbeddingAsync(question, cancellationToken: ct);
        var results =
            await vectorSearchService.HybridVectorSearch(inputEmbedding.ToArray(), collection.CollectionName, options);

        return results;
    }

    public async Task WriteAsync(string collectionName, IEnumerable<DocumentChunk> chunks, CancellationToken ct)
    {
        var collection = vectorStore.GetCollection<string, DocumentChunk>(collectionName);
        await collection.CreateCollectionIfNotExistsAsync(ct);

        foreach (var chunk in chunks)
        {
            chunk.TextEmbedding =
                await embeddingGenerationService.GenerateEmbeddingAsync(chunk.Text, cancellationToken: ct);
            await collection.UpsertAsync(chunk, ct);
        }
    }
}