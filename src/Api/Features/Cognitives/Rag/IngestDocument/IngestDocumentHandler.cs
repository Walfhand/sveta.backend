using System.Text.RegularExpressions;
using Api.Features.Chats.Agents;
using Api.Features.Cognitives.Rag.Shared.Abstractions;
using Api.Features.Cognitives.Rag.Shared.Chunks.Models;
using Api.Features.Projects.Domain.Events;
using Engine.Wolverine;
using Engine.Wolverine.Factory;
using Microsoft.SemanticKernel.Text;
using Newtonsoft.Json;

namespace Api.Features.Cognitives.Rag.IngestDocument;

#pragma warning disable SKEXP0050
public record DocumentTypeResult(string Category, decimal Score);

public class IngestDocumentHandler(IAppDbContextFactory dbContextFactory, IHttpContextAccessor contextAccessor)
    : Handler(dbContextFactory, contextAccessor)
{
    public async Task Handle(DocumentCreatedEvent domainEvent, ClassificationAgent classificationAgent,
        IRagWrite ragWrite, CancellationToken ct)
    {
        var lines = TextChunker.SplitPlainTextLines(domainEvent.Content, 1000);
        var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 300, 50).ToList();
        var textExtract = string.Join(',', paragraphs.Take(50));

        var json = await classificationAgent.DetermineCategories(textExtract, ct);
        var pattern = @"^```json\s*(.*?)\s*```$";
        var match = Regex.Match(json, pattern, RegexOptions.Singleline);
        DocumentTypeResult[] documentTypeResults = [];
        if (match.Success)
        {
            var cleanJson = match.Groups[1].Value.Trim();
            documentTypeResults = JsonConvert.DeserializeObject<DocumentTypeResult[]>(cleanJson)!;
        }

        var documentChunks = paragraphs.Select((t, chunkIndex) => new DocumentChunk
        {
            Key = $"{domainEvent.DocumentId.Value}_c{chunkIndex}",
            DocumentName = domainEvent.Name,
            ChunkNumber = chunkIndex,
            Text = t,
            Category1 = documentTypeResults[0].Category,
            Category2 = documentTypeResults[1].Category,
            ScoreCategory1 = documentTypeResults[0].Score,
            ScoreCategory2 = documentTypeResults[1].Score
        }).ToList();

        await ragWrite.WriteAsync(domainEvent.ProjectId.Value.ToString(), documentChunks, ct);
    }
}