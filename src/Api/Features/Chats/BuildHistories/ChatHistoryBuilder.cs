using Api.Features.Projects.Domain.Entities;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Api.Features.Chats.BuildHistories;

public static class ChatHistoryBuilder
{
    public static ChatHistory BuildChatHistory(this Conversation conversation, string question,
        string context, string systemPrompt)
    {
        ChatHistory chatHistory = [];

        chatHistory.AddSystemMessage(context);
        chatHistory.AddSystemMessage(systemPrompt);

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
}