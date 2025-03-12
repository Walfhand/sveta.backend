using System.Net;
using Engine.Core.Models;

namespace Engine.Exceptions;

public class NotFoundException
    : CustomException
{
    public NotFoundException(Type entityType, IdBase id) : base($"The {entityType.Name} with id {id.Value} not found",
        HttpStatusCode.NotFound)
    {
    }

    public NotFoundException() : base("", HttpStatusCode.NotFound)
    {
    }
}

public static class EntityExtensions
{
    public static NotFoundException NotFound<TId, TEntity>(this TEntity? entity, TId id)
        where TId : IdBase, new()
        where TEntity : Entity<TId>
    {
        return new NotFoundException(typeof(TEntity), id);
    }

    public static NotFoundException NotFound<TEntity>(this TEntity? entity)
    {
        return new NotFoundException();
    }
}