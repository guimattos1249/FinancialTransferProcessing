namespace FinancialTransferProcessing.Domain.Entities;

public abstract class EntityBase
{
    public EntityBase()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = CreatedAt;
    }
    public Guid Id { get; protected set; }
    public DateTimeOffset CreatedAt { get; protected set; }
    public DateTimeOffset UpdatedAt { get; protected set; }
    public DateTimeOffset? DeletedAt { get; protected set; }
}

