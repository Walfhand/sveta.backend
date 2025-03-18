using Api.Features.Projects.Domain.Entities;
using DomainEssentials.Core.Events;

namespace Api.Features.Projects.Domain.Events;

public record DocumentCreatedEvent(ProjectId ProjectId, DocumentId DocumentId, string Name, string Content)
    : IDomainEvent;