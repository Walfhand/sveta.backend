namespace Engine.Core.Events;

public interface IEventMapper
{
    IIntegrationEvent? MapToIntegrationEvent(IDomainEvent domainEvent);
}