using Api.Features.Projects.Domain;
using Api.Features.Projects.Domain.Entities;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Api.Features.Projects.Features.Conversations.Agents;

public class DecisionAgent(
    [FromKeyedServices("decision")] IChatCompletionService chatCompletionService,
    BusinessAgent businessAgent)
{
    private static Dictionary<string, string> AvailableAgents()
    {
        return new Dictionary<string, string>
        {
            { nameof(BusinessAgent), BusinessAgent.Description },
            { nameof(OnboardingAgent), OnboardingAgent.Description }
        };
    }

    public async Task<string> RouteAndAnswerAsync(
        string question,
        ProjectId projectId,
        string projectName,
        Conversation conversation,
        CancellationToken ct)
    {
        var agentName = await DetermineAgentAsync(question, ct);
        return agentName switch
        {
            nameof(BusinessAgent) =>
                await businessAgent.AnswerAsync(projectId, projectName, question, conversation, ct),
            _ => throw new NotImplementedException()
        };
    }

    private async Task<string> DetermineAgentAsync(string question, CancellationToken ct)
    {
        ChatHistory chatHistory = [];

        chatHistory.AddSystemMessage(SystemPrompt());
        chatHistory.AddUserMessage($"Question: {question}");

        var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);

        var agentName = response.Content!.Trim();
        return AvailableAgents().ContainsKey(agentName) ? agentName : nameof(BusinessAgent);
    }

    private string SystemPrompt()
    {
        return $"""
                Tu es un agent de décision qui doit déterminer quel agent spécialisé est le plus approprié pour répondre à une question.
                Tu dois choisir parmi les agents suivants :
                {string.Join("\n", AvailableAgents().Select(keyValue => $"- {keyValue.Key}"))}

                Voici une description de chaque agent :
                {string.Join("\n", AvailableAgents().Select(keyValue => $"- {keyValue.Key} : {keyValue.Value}"))}

                Réponds uniquement avec le nom exact de l'agent qui devrait traiter cette question, sans aucune explication.
                """;
    }
}