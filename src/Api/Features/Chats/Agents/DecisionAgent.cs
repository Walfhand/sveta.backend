using Microsoft.SemanticKernel.ChatCompletion;

namespace Api.Features.Chats.Agents;

public class DecisionAgent(
    [FromKeyedServices("decision")] IChatCompletionService chatCompletionService,
    BusinessAgent businessAgent,
    OnboardingAgent onboardingAgent,
    CodeAgent codeAgent)
{
    private Dictionary<string, string> AvailableAgents()
    {
        return new Dictionary<string, string>
        {
            { nameof(BusinessAgent), businessAgent.Description },
            { nameof(OnboardingAgent), onboardingAgent.Description },
            { nameof(CodeAgent), codeAgent.Description }
        };
    }

    public async Task<IAgent> GetBestAgentAsync(string question, CancellationToken ct)
    {
        var agentName = await DetermineAgentAsync(question, ct);
        return agentName switch
        {
            nameof(BusinessAgent) => businessAgent,
            nameof(OnboardingAgent) => onboardingAgent,
            nameof(CodeAgent) => codeAgent,
            _ => throw new ArgumentOutOfRangeException(agentName)
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