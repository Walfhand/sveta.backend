using Api.Features.Projects.Domain;
using Api.Features.Projects.Features.Documents.ChunkDocument.Models;
using Api.Shared.Rag.Abstractions;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;

namespace Api.Shared.Rag.Implementations;
#pragma warning disable SKEXP0001
public class RagService(IVectorStore vectorStore, ITextEmbeddingGenerationService embeddingGenerationService)
    : IRagRead, IRagWrite
{
    public async Task<List<(string key, string value, string link)>> ReadAsync(ProjectId projectId,
        string question, CancellationToken ct)
    {
        List<(string key, string value, string link)> results = [];
        var collection = vectorStore.GetCollection<string, DocumentChunk>(projectId.Value.ToString());
        var textSearch = new VectorStoreTextSearch<DocumentChunk>(collection, embeddingGenerationService);

        var searchResults =
            await textSearch.GetTextSearchResultsAsync(question,
                new TextSearchOptions { Top = 10 }, ct);

        await foreach (var result in searchResults.Results.WithCancellation(ct))
            results.Add((result.Name!, result.Value, result.Link!));
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
            await collection.UpsertAsync(chunk, cancellationToken: ct);
        }
    }
}