using Api.Features.Projects.Domain.Entities;
using Api.Features.Projects.Domain.Events;
using Api.Features.Projects.Domain.ValueObjects;
using Api.Shared.Files.Extractors.Abstractions;
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

    public Document AddDocument(string name, byte[] content, IFileContentExtractor fileContentExtractor,
        string contentType)
    {
        var document = Document.Create(name, content, contentType);
        _documents.Add(document);
        AddDomainEvent(new DocumentCreatedEvent(Id, document.Id, document.Name, fileContentExtractor.Extract(content)));
        return document;
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