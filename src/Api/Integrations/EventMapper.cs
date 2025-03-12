using Engine.Core.Events;

namespace Api.Integrations;

public class EventMapper : IEventMapper
{
    public IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        return null;
    }
}