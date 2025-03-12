using Api.Features.Projects.Domain;
using Api.Features.Projects.Features.Documents.ChunkDocument.Models;
using Engine.Wolverine;
using Engine.Wolverine.Factory;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Microsoft.SemanticKernel.PromptTemplates.Handlebars;
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Projects.Features.Conversations.Ask.Endpoints;
#pragma warning disable SKEXP0001
public record AskRequest
{
    [FromRoute] public Guid Id { get; set; }
    [FromBody] public AskRequestBody Body { get; set; } = null!;

    public record AskRequestBody
    {
        public string Question { get; set; } = null!;
    }
}

public record AskResponse
{
    public string Message { get; set; } = null!;
}

public class AskEndpoint()
    : PostMinimalEndpoint<AskRequest, AskResponse>("projects/{projectId}conversations/{conversationId:guid}/ask");

public class AskRequestHandler(IAppDbContextFactory dbContextFactory, IHttpContextAccessor contextAccessor)
    : Handler(dbContextFactory, contextAccessor)
{
    public async Task<AskResponse> Handle(AskRequest request, Kernel kernel,
        ITextEmbeddingGenerationService textEmbeddingGenerationService, IVectorStore vectorStore, CancellationToken ct)
    {
        var project = await DbContext.Set<Project>().FirstAsync(x => x.Id == request.Id, ct);
        var collection = vectorStore.GetCollection<string, DocumentChunk>(request.Id.ToString());
        var textSearch = new VectorStoreTextSearch<DocumentChunk>(collection, textEmbeddingGenerationService);
        if (kernel.Plugins.FirstOrDefault(x => x.Name == "SearchPlugin") is null)
        {
            var searchPlugin = textSearch.CreateWithGetTextSearchResults("SearchPlugin");
            kernel.Plugins.Add(searchPlugin);
        }
        
        ChatHistory chatHistory = [];

        var promptTemplate = """
                             CONTEXT:
                             -----------------------------
                             {{#with (SearchPlugin-GetTextSearchResults query count)}}  
                               {{#each this}}  
                                 Name: {{Name}}
                                 Value: {{Value}}
                                 Link: {{Link}}
                                 -----------------
                               {{/each}}  
                             {{/with}}  
                             Question : 
                             ------------------------------------------------
                             {{query}}
                             ------------------------------------------------
                             Nous sommes dans un contexte qui touche à un projet IT qui s'appelle {{projectName}}
                             Réponds à la question en te basant uniquement sur les extraits fournis ci-dessus. 
                             Si tu ne trouves pas la réponse dans les extraits ou si c'est hors contexte par rapport à la question alors tu utilise tout ton savoir pour y répondre.
                             Tu ne dira pas 'D'après les extraits fournis.', ça doit être transparent pour le client.
                             Tu répondras dans la langue de la question.
                             """;

        KernelArguments arguments = new()
        {
            { "query", request.Body.Question },
            { "count", 20 },
            { "projectName", project.Name }
        };
        HandlebarsPromptTemplateFactory promptTemplateFactory = new();


        var result = await kernel.InvokePromptAsync(
            promptTemplate,
            arguments,
            HandlebarsPromptTemplateFactory.HandlebarsTemplateFormat,
            promptTemplateFactory, ct
        );


        return new AskResponse
        {
            Message = result.ToString()
        };
    }
}