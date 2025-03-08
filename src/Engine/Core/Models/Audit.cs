namespace Engine.Core.Models;

public abstract class Audit : IAudit
{
    public DateTime? CreatedAt { get; set; }
    public long? CreatedBy { get; set; }
    public DateTime? LastModified { get; set; }
    public Guid? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }
    public long Version { get; set; } = -1;
}