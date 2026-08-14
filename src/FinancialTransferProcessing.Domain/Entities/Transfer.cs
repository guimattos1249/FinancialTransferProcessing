using FinancialTransferProcessing.Domain.Enums;
using FinancialTransferProcessing.Domain.Exceptions;
using FinancialTransferProcessing.Domain.Validations;

namespace FinancialTransferProcessing.Domain.Entities;

public class Transfer : EntityBase
{
    public const int MaxIdempotencyKeyLength = 100;

    public Transfer(
        Guid payerId, 
        Guid payeeId,
        long amountInCents,
        string idempotencyKey)
    {
        ValidatePayerEqualsPayee(payerId, payeeId);
        Status = ETransferStatus.Pending;
        SetPayer(payerId);
        SetPayee(payeeId);
        SetAmountInCents(amountInCents);
        IdempotencyKey = ValidateIdempotencyKey(idempotencyKey);
    }

    public Guid PayerId { get; private set; }
    public Guid PayeeId { get; private set; }
    public long AmountInCents { get; private set; }
    public string IdempotencyKey { get; private set; }
    public ETransferStatus Status { get; private set; }
    public DateTimeOffset? ProcessedAt { get; private set; }
    public string? FailureReason { get; private set; }
    public string? CorrelationId { get; private set; }

    public Account Payer { get; private set; } = null!;
    public Account Payee { get; private set; } = null!;

    private void SetPayer(Guid payerId)
    {
        if (payerId == Guid.Empty)
            throw new DomainException("Payer cannot be empty.");

        PayerId = payerId;
    }

    private void SetPayee(Guid payeeId)
    {
        if (payeeId == Guid.Empty)
            throw new DomainException("Payee cannot be empty.");

        PayeeId = payeeId;
    }

    private void SetAmountInCents(long amountInCents)
    {
        DomainValidation.EnsurePositiveAmount(amountInCents);

        AmountInCents = amountInCents;
    }

    private static string ValidateIdempotencyKey(string idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new DomainException("Idempotency key cannot be empty.");

        idempotencyKey = idempotencyKey.Trim();

        if (idempotencyKey.Length > MaxIdempotencyKeyLength)
            throw new DomainException(
                "Idempotency key cannot exceed 100 characters.");

        return idempotencyKey;
    }

    private static void ValidatePayerEqualsPayee(Guid payerId, Guid payeeId)
    {
        if (payerId == payeeId)
            throw new DomainException(
                "Payer and payee must be different accounts.");
    }

    public void Complete()
    {
        if (Status != ETransferStatus.Pending)
            throw new DomainException("Only pending tranfers can be completed.");

        Status = ETransferStatus.Completed;
        FailureReason = null;
        UpdatedAt = DateTimeOffset.UtcNow;
        ProcessedAt = UpdatedAt;
    }

    public void Fail(string reason)
    {
        if (Status != ETransferStatus.Pending)
            throw new DomainException("Only pending tranfers can be failed.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new DomainException("Failure reason cannot be empty");

        Status = ETransferStatus.Failed;
        FailureReason = reason.Trim();
        UpdatedAt = DateTimeOffset.UtcNow;
        ProcessedAt = UpdatedAt;
    }
}
