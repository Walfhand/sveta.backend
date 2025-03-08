using Microsoft.AspNetCore.Mvc;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Memory;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Projects.Features.Documents.UploadDocuments.Endpoints;

#pragma warning disable SKEXP0001
public class SaveDocumentsRequest
{
    [FromBody] public SaveMomoryRequestBody Body { get; set; } = null!;

    public record SaveMomoryRequestBody
    {
        public string Id { get; set; } = null!;
        public string Text { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Collection { get; set; } = null!;
    }
}

public record SaveDocumentsResponse
{
}

public class UploadDocumentsEndpoint()
    : PostMinimalEndpoint<SaveDocumentsRequest, SaveDocumentsResponse>("projects/{id:guid}/documents")
{
    private const string DefaultCollection = "api_knowledge_base";
    protected override Delegate Handler => Endpoint;

    private static async Task<IResult> Endpoint([AsParameters] SaveDocumentsRequest request, Kernel kernel,
        ISemanticTextMemory memory, CancellationToken ct)
    {
        try
        {
            var collection = string.IsNullOrEmpty(request.Body.Collection)
                ? DefaultCollection
                : request.Body.Collection;
            await memory.SaveInformationAsync(
                collection,
                id: request.Body.Id,
                text: request.Body.Text,
                description: request.Body.Description,
                cancellationToken: ct
            );

            return Results.Ok(new { message = "Information saved successfully" });
        }
        catch (Exception ex)
        {
            return Results.InternalServerError(ex.Message);
        }
    }
}