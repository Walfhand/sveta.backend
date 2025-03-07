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

    public List<Document> Documents { get; private set; } = [];
    public IReadOnlyCollection<Conversation> Conversations => _conversations.AsReadOnly();

    public void StartNewConversation(string collection)
    {
        _conversations.Add(Conversation.Create(collection));
    }

    public static Project Create(string name, string description)
    {
        return new Project(name, description);
    }
}