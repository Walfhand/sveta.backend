using Api.Features.Cognitives.Rag.Shared.Abstractions;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Api.Features.Chats.Agents;

public class ClassificationAgent(
    Kernel kernel,
    [FromKeyedServices("classification")] IChatCompletionService chatCompletionService,
    IRagRead ragRead)
{
    public async Task<string> DetermineCategories(string documentContent, CancellationToken ct)
    {
        ChatHistory chatHistory = [];

        chatHistory.AddSystemMessage(SystemPrompt());
        chatHistory.AddUserMessage($"Document content: {documentContent}");

        var response = await chatCompletionService.GetChatMessageContentAsync(chatHistory, cancellationToken: ct);
        return response.Content!;
    }

    private static string SystemPrompt()
    {
        return """
                You're a classification assistant.
                You need to be able to tell if a text is relevant to one or more of the categories below.
                ATTENTION: there may be more than one, as one text may correspond to several categories.

               Your answer must contain only json with an object array containing 2 properties:
               - category, which represents one of the categories below
               - score which represents a relevance score ranging from 0 to 1
               exemple of json: 
               
                "[
                    {
                        "category":"category",
                        "score": 0.5
                    }     
                ]"
                
                here's the list of categories:
                   - business
                   - code
                   - technicalAnalysis
                   - documentation
                
                IMPORTANT: ONLY THE TWO BEST WILL BE TAKEN The array must contain only 2 elements
                ATTENTION: THE SCORE IS VERY IMPORTANT EVALUATE THE GOOD
                CAUTION: NEVER REPLY WITH ANYTHING OTHER THAN JSON
                YOU WILL NOT GIVE ANY EXPLANATION OF YOUR CHOICES, YOU WILL ONLY ANSWER WITH JSON.
                YOU WON'T NEED A SEPARATOR ```JSON
               """;
    }
}