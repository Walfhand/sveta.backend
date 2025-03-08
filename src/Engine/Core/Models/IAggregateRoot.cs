using Engine.Core.Events;

namespace Engine.Core.Models;

public interface IAggregateRoot : IAudit
{
    IReadOnlyList<IDomainEvent> DomainEvents { get; }
    IEvent[] ClearDomainEvents();
}