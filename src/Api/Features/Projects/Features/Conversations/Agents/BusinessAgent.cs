using System.Text;
using Api.Features.Projects.Domain;
using Api.Features.Projects.Domain.Entities;
using Api.Features.Projects.Features.Documents.ChunkDocument.Models;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;

namespace Api.Features.Projects.Features.Conversations.Agents;
#pragma warning disable SKEXP0001
public class BusinessAgent(
    ITextEmbeddingGenerationService textEmbeddingGenerationService,
    IVectorStore vectorStore,
    [FromKeyedServices("business")] IChatCompletionService chatCompletionService)
{
    public const string Description =
        "Spécialiste en gestion de projet IT, analyse métier, processus business, besoins fonctionnels";

    public async Task<string> AnswerAsync(ProjectId projectId, string projectName, string question,
        Conversation conversation,
        CancellationToken ct)
    {
        var chatHistory = await BuildChatHistoryAsync(projectId, question, projectName, conversation, ct);
        var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        return response.Content!;
    }

    private async Task<string> SearchInMemoryAsync(string question, ProjectId projectId, CancellationToken ct)
    {
        var collection = vectorStore.GetCollection<string, DocumentChunk>(projectId.Value.ToString());
        var textSearch = new VectorStoreTextSearch<DocumentChunk>(collection, textEmbeddingGenerationService);

        var resultsDic = new Dictionary<string, List<(string value, string link)>>();
        var searchResults =
            await textSearch.GetTextSearchResultsAsync(question, new TextSearchOptions { Top = 20 }, ct);

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

        return formattedResults.ToString();
    }

    private async Task<ChatHistory> BuildChatHistoryAsync(ProjectId projectId, string question, string projectName,
        Conversation conversation, CancellationToken ct)
    {
        ChatHistory chatHistory = [];

        chatHistory.AddSystemMessage(await SearchInMemoryAsync(question, projectId, ct));
        chatHistory.AddSystemMessage(SystemPrompt(projectName));

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

        chatHistory.AddUserMessage(question);
        return chatHistory;
    }

    private static string SystemPrompt(string projectName)
    {
        return $"""
                Tu es une assistante spécialiste en gestion de projet IT et en développement et ton nom est SVETA 
                ce nom n'est pas le diminutif de Svetlana, si on te demande ton nom, tu diras simplement 'SVETA'.
                Tu t'adresse au gens de façon chaleureuse et amicale, tu dois tutoyer les gens.
                Nous sommes dans un contexte qui touche à un projet IT qui s'appelle {projectName}

                Les extraits de code et de documentation fournis sont organisés par fichier et position dans le fichier.
                Lorsque tu analyses ces extraits, prends en compte leur ordre d'apparition dans chaque fichier
                pour comprendre la structure et le flux du code.

                Réponds à la question en te basant uniquement sur les extraits fournis ci-dessus. 
                Si tu ne trouves pas la réponse dans les extraits ou si c'est hors contexte par rapport à la question
                alors tu diras que tu ne sais pas.

                Tu ne diras pas 'D'après les extraits fournis.', ça doit être transparent pour le client.
                Tu répondras dans la langue de la question.
                """;
    }
}