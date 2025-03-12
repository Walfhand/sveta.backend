using Api.Features.Projects.Domain.ValueObjects;
using Engine.Core.Models;

namespace Api.Features.Projects.Domain.Entities;

public record ConversationId : IdBase
{
    public ConversationId()
    {
    }

    public ConversationId(Guid id) : base(id)
    {
    }
}

public class Conversation : Entity<ConversationId>
{
    private readonly List<ChatMessage> _chatMessages = [];

    private Conversation()
    {
    }

    private Conversation(string collection)
    {
        Title = "New Conversation";
        Collection = collection;
    }

    public string Title { get; private set; } = null!;
    public string Collection { get; private set; } = null!;
    public IReadOnlyCollection<ChatMessage> ChatMessages => _chatMessages.AsReadOnly();

    public static Conversation Create(string collection)
    {
        return new Conversation(collection);
    }


    public void AddChatMessage(string role, string message)
    {
        var lastOrder = _chatMessages.OrderBy(x => x.Order).LastOrDefault()?.Order ?? 0;
        _chatMessages.Add(new ChatMessage(role, message, lastOrder + 1));
    }
}