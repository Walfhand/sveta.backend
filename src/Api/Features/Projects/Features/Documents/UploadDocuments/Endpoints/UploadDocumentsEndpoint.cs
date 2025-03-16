using Api.Features.Projects.Domain;
using Api.Features.Projects.Domain.Entities;
using Api.Features.Projects.Features.Documents.ChunkDocument.Models;
using Api.Shared.Files;
using Api.Shared.Rag.Abstractions;
using Engine.Exceptions;
using Engine.Wolverine;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel.Text;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Projects.Features.Documents.UploadDocuments.Endpoints;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0050
public class SaveDocumentsRequest
{
    [FromForm] public IFormFile File { get; set; } = null!;
    [FromRoute] public Guid Id { get; set; }

    public record SaveDocumentsRequestBody
    {
        public IFormFile File { get; set; } = null!;
    }
}

public record SaveDocumentsResponse
{
    public string Message { get; set; } = null!;
}

public class UploadDocumentsEndpoint()
    : PostMinimalEndpoint<SaveDocumentsRequest, SaveDocumentsResponse>("projects/{id:guid}/documents")
{
    protected override RouteHandlerBuilder Configure(IEndpointRouteBuilder builder)
    {
        var test = base.Configure(builder);
        test.DisableAntiforgery();
        return test;
    }
}

public class SaveDocumentsRequestHandler(IAppDbContextFactory dbContextFactory, IHttpContextAccessor contextAccessor)
    : Handler(dbContextFactory, contextAccessor)
{
    public async Task<SaveDocumentsResponse> Handle(SaveDocumentsRequest request,
        PdfContentExtractor pdfExtractor, IRagWrite ragWrite, CancellationToken ct)
    {
        var project = await DbContext.Set<Project>().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (project is null)
            throw project.NotFound(new ProjectId(request.Id));

        using var stream = new MemoryStream();
        await request.File.CopyToAsync(stream, ct);
        var fileBytes = stream.ToArray();
        var document = Document.Create(request.File.FileName, fileBytes, request.File.ContentType);
        var pdfContent = pdfExtractor.ExtractContent(fileBytes);

        var documentChunks = new List<DocumentChunk>();

        var lines = TextChunker.SplitPlainTextLines(pdfContent.Text, 150);
        for (var chunkIndex = 0; chunkIndex < lines.Count; chunkIndex++)
        {
            var chunk = new DocumentChunk
            {
                Key = $"{document.Id}_c{chunkIndex}",
                DocumentName = document.Name,
                ChunkNumber = chunkIndex,
                Text = lines[chunkIndex]
            };
            documentChunks.Add(chunk);
        }

        await ragWrite.WriteAsync(project.Id.Value.ToString(), documentChunks, ct);

        return new SaveDocumentsResponse
        {
            Message = "Ok"
        };
    }
}