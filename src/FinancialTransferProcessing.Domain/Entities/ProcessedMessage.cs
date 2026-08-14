using FinancialTransferProcessing.Domain.Exceptions;

namespace FinancialTransferProcessing.Domain.Entities;

public class ProcessedMessage
{
    public ProcessedMessage(Guid messageId, Guid transferId)
    {
        if (messageId == Guid.Empty)
            throw new DomainException("Message ID cannot be empty.");

        if (transferId == Guid.Empty)
            throw new DomainException("Transfer ID cannot be empty.");

        MessageId = messageId;
        TransferId = transferId;
        ProcessedAt = DateTimeOffset.UtcNow;
    }

    public Guid MessageId { get; private set; }
    public Guid TransferId { get; private set; }
    public DateTimeOffset ProcessedAt { get; private set; }
}
