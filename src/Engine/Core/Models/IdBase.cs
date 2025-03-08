namespace Engine.Core.Models;

public record IdBase(Guid Value)
{
    protected IdBase() : this(Guid.NewGuid())
    {
    }

    public static implicit operator Guid(IdBase userId) => userId.Value;
    public static implicit operator IdBase(Guid value) => new(value);
}