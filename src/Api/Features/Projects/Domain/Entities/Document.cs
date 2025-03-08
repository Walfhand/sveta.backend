using Engine.Core.Models;

namespace Api.Features.Projects.Domain.Entities;

public record DocumentId : IdBase
{
    public DocumentId()
    {
        
    }

    public DocumentId(Guid id) : base(id)
    {
        
    }
}

public class Document : Entity<DocumentId>
{
    public string Name { get; private set; }
    public string ContentHash { get; private set; }
    public string ContentType { get; private set; }
}