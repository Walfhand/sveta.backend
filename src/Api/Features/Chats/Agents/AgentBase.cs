using System.Text;
using Api.Features.Chats.BuildHistories;
using Api.Features.Cognitives.Rag.Search;
using Api.Features.Cognitives.Rag.Shared.Abstractions;
using Api.Features.Projects.Domain;
using Api.Features.Projects.Domain.Entities;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Api.Features.Chats.Agents;

public abstract class AgentBase(Kernel kernel, IChatCompletionService chatCompletionService, IRagRead ragRead)
    : IAgent
{
    public abstract string Description { get; }

    public async Task<string> AnswerAsync(ProjectId projectId, string projectName, string question,
        Conversation conversation,
        CancellationToken ct)
    {
        var chatHistory = conversation.BuildChatHistory(question, await ConstructContext(question, projectId, ct),
            SystemPrompt(projectName));
        var response =
            await chatCompletionService.GetChatMessageContentAsync(chatHistory, kernel: kernel, cancellationToken: ct);

        return response.Content!;
    }


    private async Task<string> ConstructContext(string question, ProjectId projectId, CancellationToken ct)
    {
        var results = await ragRead.ReadAsync(projectId, question, ct, GetOptions());
        var formattedResults = new StringBuilder();
        formattedResults.AppendLine("CONTEXT:");
        formattedResults.AppendLine("-----------------");

        foreach (var grouping in results.OrderBy(x => x.TotalScore).GroupBy(x => x.DocumentName))
        {
            formattedResults.AppendLine($"Nom du document: {grouping.Key}");
            formattedResults.AppendLine("Contenu du document:");
            foreach (var value in grouping.OrderBy(x => x.ChunkNumber))
            {
                formattedResults.AppendLine("chunk: " + value.ChunkNumber);
                formattedResults.AppendLine("==================================================================");
                formattedResults.AppendLine(value.Text);
            }

            formattedResults.AppendLine("-----------------");
        }

        return formattedResults.ToString();
    }

    protected abstract string SystemPrompt(string projectName);
    protected abstract VectorSearchOptions GetOptions();
}