using Api.Features.Projects.Domain;
using Api.Features.Projects.Features.Documents.UploadDocuments.Results;
using Api.Shared.Files.Extractors.Abstractions;
using Engine.Exceptions;
using Engine.Wolverine;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Projects.Features.Documents.UploadDocuments.Endpoints;

#pragma warning disable SKEXP0001
#pragma warning disable SKEXP0050
public class SaveDocumentsRequest
{
    [FromForm] public IFormFile File { get; set; } = null!;
    [FromRoute] public Guid Id { get; set; }
}

public class UploadDocumentsEndpoint()
    : PostMinimalEndpoint<SaveDocumentsRequest, DocumentResult>("projects/{id:guid}/documents")
{
    protected override RouteHandlerBuilder Configure(IEndpointRouteBuilder builder)
    {
        var routeBuilder = base.Configure(builder);
        routeBuilder.DisableAntiforgery();
        return routeBuilder;
    }
}

public class SaveDocumentsRequestHandler(IAppDbContextFactory dbContextFactory, IHttpContextAccessor contextAccessor)
    : Handler(dbContextFactory, contextAccessor)
{
    public async Task<DocumentResult> Handle(SaveDocumentsRequest request,
        IFileContentExtractor fileContentExtractor, CancellationToken ct)
    {
        var project = await DbContext.Set<Project>().FirstOrDefaultAsync(x => x.Id == request.Id, ct);
        if (project is null)
            throw project.NotFound(new ProjectId(request.Id));

        using var stream = new MemoryStream();
        await request.File.CopyToAsync(stream, ct);
        var fileBytes = stream.ToArray();

        var document = project.AddDocument(request.File.FileName, fileBytes, fileContentExtractor,
            request.File.ContentType);

        return document.ToResult();
    }
}