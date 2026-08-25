using FinancialTransferProcessing.Domain.Exceptions;

namespace FinancialTransferProcessing.Domain.Validations;

public static class DomainValidation
{
    public const int MaxCorrelationIdLength = 100;

    public static void EnsurePositiveAmount(long amountInCents)
    {
        if (amountInCents <= 0)
            throw new DomainException("Amount must be greater than zero");
    }

    public static string ValidateCorrelationId(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            throw new DomainException("Correlation ID cannot be empty.");

        var normalizedCorrelationId = correlationId.Trim();

        if (normalizedCorrelationId.Length > MaxCorrelationIdLength)
            throw new DomainException($"Correlation ID cannot exceed {MaxCorrelationIdLength} characters.");

        return normalizedCorrelationId;
    }
}
