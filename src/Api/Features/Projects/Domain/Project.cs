using Api.Features.Projects.Domain.Entities;
using Api.Features.Projects.Domain.ValueObjects;
using Engine.Core.Models;

namespace Api.Features.Projects.Domain;

public record ProjectId : IdBase
{
    public ProjectId()
    {
    }

    public ProjectId(Guid id) : base(id)
    {
    }
}

public class Project : AggregateRoot<ProjectId>
{
    private readonly List<Conversation> _conversations = [];
    private readonly List<Document> _documents = [];

    private Project()
    {
    }

    private Project(string name, string description)
    {
        Name = name;
        Description = description;
    }

    public string Name { get; private set; } = null!;
    public string Description { get; private set; } = null!;
    public GithubUrl? GithubUrl { get; private set; }


    public IReadOnlyCollection<Conversation> Conversations => _conversations.AsReadOnly();
    public IReadOnlyCollection<Document> Documents => _documents.AsReadOnly();

    public void AddDocument(string name, byte[] content, string contentType)
    {
        _documents.Add(Document.Create(name, content, contentType));
    }

    public Conversation StartNewConversation()
    {
        var conversation = Conversation.Create("");
        _conversations.Add(conversation);
        return conversation;
    }

    public Conversation GetConversation(ConversationId id)
    {
        return _conversations.Single(x => x.Id == id);
    }

    public static Project Create(string name, string description)
    {
        return new Project(name, description);
    }
}