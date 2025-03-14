using System.Text;
using Api.Features.Projects.Domain;
using Api.Features.Projects.Domain.Entities;
using Api.Features.Projects.Features.Conversations.BuildHistories;
using Api.Rag.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Api.Features.Projects.Features.Conversations.Agents;

public abstract class AgentBase(Kernel kernel, IChatCompletionService chatCompletionService, IRagRead ragRead)
    : IAgent
{
    protected readonly IChatCompletionService ChatCompletionService = chatCompletionService;
    protected readonly Kernel Kernel = kernel;
    protected readonly IRagRead RagRead = ragRead;

    public abstract string Description { get; }

    public async Task<string> AnswerAsync(ProjectId projectId, string projectName, string question,
        Conversation conversation,
        CancellationToken ct)
    {
        var chatHistory = conversation.BuildChatHistory(question, await ConstructContext(question, projectId, ct),
            SystemPrompt(projectName));
        var response =
            await ChatCompletionService.GetChatMessageContentAsync(chatHistory, kernel: Kernel,
                executionSettings: new OpenAIPromptExecutionSettings
                {
                    MaxTokens = 8000
                }, cancellationToken: ct);

        return response.Content!;
    }


    protected async Task<string> ConstructContext(string question, ProjectId projectId, CancellationToken ct)
    {
        var results = await RagRead.ReadAsync(projectId, question, ct);
        var formattedResults = new StringBuilder();
        formattedResults.AppendLine("CONTEXT:");
        formattedResults.AppendLine("-----------------");

        foreach (var grouping in results.GroupBy(x => x.key))
        {
            formattedResults.AppendLine($"Nom du document: {grouping.Key}");
            formattedResults.AppendLine("Contenu du document:");
            foreach (var value in grouping.OrderBy(x => x.link)) formattedResults.AppendLine(value.value);
            formattedResults.AppendLine("-----------------");
        }

        return formattedResults.ToString();
    }

    protected abstract string SystemPrompt(string projectName);
}