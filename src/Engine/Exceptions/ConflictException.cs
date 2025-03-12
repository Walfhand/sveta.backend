using System.Reflection;

namespace Engine.Exceptions;

public class ConflictException(MemberInfo entityType, string propertyName)
    : CustomException($"Conflict{entityType.Name}{propertyName}")
{
}

public static class ConflictExceptionExtensions
{
    public static ConflictException Conflict(this object obj, string propertyName)
    {
        return new ConflictException(obj.GetType(), propertyName);
    }
}