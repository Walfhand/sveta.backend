using Api.Features.Projects.Domain.Entities;

namespace Api.Features.Chats.CreateConversation.Results;

public record ConversationResult(Guid Id, string Collection, string Title);

public static class ConversationExtensions
{
    public static ConversationResult ToResult(this Conversation conversation)
    {
        return new ConversationResult(conversation.Id, conversation.Collection, conversation.Title);
    }
}