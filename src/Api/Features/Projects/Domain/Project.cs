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
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
}