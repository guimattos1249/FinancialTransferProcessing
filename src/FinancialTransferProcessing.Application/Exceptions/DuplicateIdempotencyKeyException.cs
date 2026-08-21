namespace FinancialTransferProcessing.Application.Exceptions;

public sealed class DuplicateIdempotencyKeyException()
    : ApplicationException(
        "The idempotency key already exists.");
