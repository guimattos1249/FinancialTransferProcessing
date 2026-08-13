using FinancialTransferProcessing.Domain.Enums;

namespace FinancialTransferProcessing.Domain.Entities;

public class Transfer : EntityBase
{
    public Guid PayerId { get; private set; }
    public Guid PayeeId { get; private set; }
    public long AmountInCents { get; private set; }
    public string IdempotencyKey { get; private set; }
    public ETransferStatus Status { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string? CorrelationId { get; private set; }

    public Account Payer { get; private set; }
    public Account Payee { get; private set; }
}
