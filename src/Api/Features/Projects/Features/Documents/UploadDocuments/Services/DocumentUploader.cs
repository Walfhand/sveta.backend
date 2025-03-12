using Api.Features.Projects.Features.Documents.ChunkDocument.Models;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.Embeddings;

namespace Api.Features.Projects.Features.Documents.UploadDocuments.Services;

#pragma warning disable SKEXP0001
public class DocumentUploader(IVectorStore vectorStore, ITextEmbeddingGenerationService embeddingGenerationService)
{
    public async Task UploadDocumentChunks(string collectionName, IEnumerable<DocumentChunk> chunks)
    {
        var collection = vectorStore.GetCollection<string, DocumentChunk>(collectionName);
        await collection.CreateCollectionIfNotExistsAsync();

        foreach (var chunk in chunks)
        {
            chunk.TextEmbedding = await embeddingGenerationService.GenerateEmbeddingAsync(chunk.Text);
            await collection.UpsertAsync(chunk);
        }
    }
}