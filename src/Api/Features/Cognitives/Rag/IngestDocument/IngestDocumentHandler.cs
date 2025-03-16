using Api.Features.Cognitives.Rag.Shared.Abstractions;
using Api.Features.Cognitives.Rag.Shared.Chunks.Models;
using Api.Features.Projects.Domain.Events;
using Engine.Wolverine;
using Engine.Wolverine.Factory;
using Microsoft.SemanticKernel.Text;

namespace Api.Features.Cognitives.Rag.IngestDocument;

#pragma warning disable SKEXP0050
public class IngestDocumentHandler(IAppDbContextFactory dbContextFactory, IHttpContextAccessor contextAccessor)
    : Handler(dbContextFactory, contextAccessor)
{
    public async Task Handle(DocumentCreatedEvent domainEvent, IRagWrite ragWrite, CancellationToken ct)
    {
        var lines = TextChunker.SplitPlainTextLines(domainEvent.Content, 150);
        var documentChunks = lines.Select((t, chunkIndex) => new DocumentChunk
        {
            Key = $"{domainEvent.DocumentId.Value}_c{chunkIndex}", DocumentName = domainEvent.Name,
            ChunkNumber = chunkIndex, Text = t
        }).ToList();

        await ragWrite.WriteAsync(domainEvent.DocumentId.Value.ToString(), documentChunks, ct);
    }
}