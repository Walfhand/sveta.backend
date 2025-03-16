using Api.Features.Projects.Domain.Entities;
using Engine.Core.Events;

namespace Api.Features.Projects.Domain.Events;

public record DocumentCreatedEvent(DocumentId DocumentId, string Name, string Content) : IDomainEvent;