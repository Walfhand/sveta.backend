using System.Text;
using Api.Features.Projects.Domain;
using Api.Features.Projects.Domain.Entities;
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
using QuickApi.Engine.Web.Endpoints.Impl;

namespace Api.Features.Projects.Features.Conversations.Ask.Endpoints;
#pragma warning disable SKEXP0001
public record AskRequest
{
    [FromRoute] public Guid ProjectId { get; set; }
    [FromRoute] public Guid ConversationId { get; set; }
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
    : PostMinimalEndpoint<AskRequest, AskResponse>("projects/{projectId}/conversations/{conversationId:guid}/ask");

public class AskRequestHandler(IAppDbContextFactory dbContextFactory, IHttpContextAccessor contextAccessor)
    : Handler(dbContextFactory, contextAccessor)
{
    public async Task<AskResponse> Handle(AskRequest request, Kernel kernel,
        ITextEmbeddingGenerationService textEmbeddingGenerationService, IVectorStore vectorStore,
        IChatCompletionService chatCompletionService, CancellationToken ct)
    {
        var project = await DbContext.Set<Project>()
            .Include(x => x.Conversations.Where(c => c.Id == request.ConversationId))
            .ThenInclude(m => m.ChatMessages)
            .AsSplitQuery()
            .FirstAsync(x => x.Id == request.ProjectId, ct);

        var collection = vectorStore.GetCollection<string, DocumentChunk>(request.ProjectId.ToString());
        var textSearch = new VectorStoreTextSearch<DocumentChunk>(collection, textEmbeddingGenerationService);
        ChatHistory chatHistory = [];

        var resultsDic = new Dictionary<string, List<(string value, string link)>>();
        var searchResults =
            await textSearch.GetTextSearchResultsAsync(request.Body.Question, new TextSearchOptions { Top = 50 }, ct);

        await foreach (var result in searchResults.Results.WithCancellation(ct))
            if (!resultsDic.TryGetValue(result.Name!, out var value))
                resultsDic.Add(result.Name!, [(result.Value, result.Link!)]);
            else
                value.Add((result.Value, result.Link!));

        var formattedResults = new StringBuilder();
        formattedResults.AppendLine("CONTEXT:");
        formattedResults.AppendLine("-----------------");

        foreach (var (key, values) in resultsDic)
        {
            formattedResults.AppendLine($"Nom du document: {key}");
            formattedResults.AppendLine("Contenu du document:");
            foreach (var value in values.OrderBy(x => x.link)) formattedResults.AppendLine(value.value);

            formattedResults.AppendLine("-----------------");
        }

        var systemMessage = $"""
                             Ton nom est SVETA, si on te demande quel est ton nom, tu le diras.
                             Tu es une assistante spécialiste en gestion de projet IT et en développement.
                             Nous sommes dans un contexte qui touche à un projet IT qui s'appelle {project.Name}
                             Réponds à la question en te basant uniquement sur les extraits fournis ci-dessus. 
                             Si tu ne trouves pas la réponse dans les extraits ou si c'est hors contexte par rapport à la question alors tu utilise tout ton savoir pour y répondre.
                             Tu ne dira pas 'D'après les extraits fournis.', ça doit être transparent pour le client.
                             Tu répondras dans la langue de la question.
                             """;

        chatHistory.AddSystemMessage(formattedResults.ToString());
        chatHistory.AddSystemMessage(systemMessage);

        var conversation = project.GetConversation(new ConversationId(request.ConversationId));

        foreach (var chatMessage in conversation.ChatMessages.OrderBy(x => x.Order))
        {
            var role = chatMessage.Role.ToLowerInvariant() switch
            {
                "user" => AuthorRole.User,
                "assistant" => AuthorRole.Assistant,
                "system" => AuthorRole.System,
                _ => AuthorRole.User
            };
            chatHistory.AddMessage(role, chatMessage.Content);
        }

        chatHistory.AddUserMessage(request.Body.Question);
        var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        conversation.AddChatMessage("user", request.Body.Question);
        conversation.AddChatMessage("assistant", response.Content!);
        return new AskResponse
        {
            Message = response.Content!
        };
    }
}