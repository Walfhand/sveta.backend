using Engine.Core.Events;

namespace Api.Shared.Integrations;

public class EventMapper : IEventMapper
{
    public IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent)
    {
        return null;
    }
}