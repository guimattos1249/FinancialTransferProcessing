namespace FinancialTransferProcessing.Domain.Entities;

public class ProcessedMessage
{
    public Guid MessageId { get; private set; }
    public Guid TransferId { get; private set; }
    public DateTimeOffset ProcessedAt { get; private set; }
}
