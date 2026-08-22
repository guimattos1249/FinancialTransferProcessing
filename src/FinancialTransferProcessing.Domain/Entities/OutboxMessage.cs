using System.Text;
using FinancialTransferProcessing.Domain.Exceptions;

namespace FinancialTransferProcessing.Domain.Entities;

public sealed class OutboxMessage
{
    public const int MaxTypeLength = 100;
    public const int MaxCorrelationIdLength = 100;
    public const int MaxLastErrorLength = 2_000;
    public const int MaxPayloadSizeInBytes = 64 * 1024;

    public Guid MessageId { get; private set; }
    public string Type { get; private set; } = null!;
    public int SchemaVersion { get; private set; }
    public string Payload { get; private set; } = null!;
    public DateTimeOffset OccurredAt { get; private set; }
    public DateTimeOffset? NextAttemptAt { get; private set; }
    public DateTimeOffset? PublishedAt { get; private set; }
    public int AttemptCount { get; private set; }
    public string? LastError { get; private set; }
    public string CorrelationId { get; private set; } = null!;

    private OutboxMessage()
    {
    }

    public OutboxMessage(
        Guid messageId,
        string type,
        int schemaVersion,
        string payload,
        DateTimeOffset occurredAt,
        string correlationId)
    {
        MessageId = ValidateMessageId(messageId);
        Type = ValidateType(type);
        SchemaVersion = ValidateSchemaVersion(schemaVersion);
        Payload = ValidatePayload(payload);
        OccurredAt = ValidateUtcDate(occurredAt, nameof(occurredAt));
        CorrelationId = ValidateCorrelationId(correlationId);
    }

    public void RegisterFailedAttempt(
        string error,
        DateTimeOffset attemptedAt,
        DateTimeOffset nextAttemptAt)
    {
        EnsureNotPublished();

        var validatedAttemptedAt = ValidateUtcDate(attemptedAt, nameof(attemptedAt));
        var validatedNextAttemptAt = ValidateUtcDate(nextAttemptAt, nameof(nextAttemptAt));
        var validatedError = ValidateLastError(error);

        if (validatedAttemptedAt < OccurredAt)
            throw new DomainException("Attempt date cannot be earlier than the message occurrence date.");

        if (validatedNextAttemptAt <= validatedAttemptedAt)
            throw new DomainException("Next attempt date must be later than the current attempt date.");

        IncrementAttemptCount();
        LastError = validatedError;
        NextAttemptAt = validatedNextAttemptAt;
    }

    public void MarkAsPublished(DateTimeOffset publishedAt)
    {
        EnsureNotPublished();

        var validatedPublishedAt = ValidateUtcDate(publishedAt, nameof(publishedAt));

        if (validatedPublishedAt < OccurredAt)
            throw new DomainException("Publication date cannot be earlier than the message occurrence date.");

        IncrementAttemptCount();
        PublishedAt = validatedPublishedAt;
        NextAttemptAt = null;
        LastError = null;
    }

    private static Guid ValidateMessageId(Guid messageId)
    {
        if (messageId == Guid.Empty)
            throw new DomainException("Message ID cannot be empty.");

        return messageId;
    }

    private static string ValidateType(string type)
    {
        if (string.IsNullOrWhiteSpace(type))
            throw new DomainException("Message type cannot be empty.");

        var normalizedType = type.Trim();

        if (normalizedType.Length > MaxTypeLength)
            throw new DomainException($"Message type cannot exceed {MaxTypeLength} characters.");

        return normalizedType;
    }

    private static int ValidateSchemaVersion(int schemaVersion)
    {
        if (schemaVersion <= 0)
            throw new DomainException("Schema version must be greater than zero.");

        return schemaVersion;
    }

    private static string ValidatePayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            throw new DomainException("Payload cannot be empty.");

        if (Encoding.UTF8.GetByteCount(payload) > MaxPayloadSizeInBytes)
            throw new DomainException($"Payload cannot exceed {MaxPayloadSizeInBytes} bytes.");

        return payload;
    }

    private static string ValidateCorrelationId(string correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
            throw new DomainException("Correlation ID cannot be empty.");

        var normalizedCorrelationId = correlationId.Trim();

        if (normalizedCorrelationId.Length > MaxCorrelationIdLength)
            throw new DomainException($"Correlation ID cannot exceed {MaxCorrelationIdLength} characters.");

        return normalizedCorrelationId;
    }

    private static string ValidateLastError(string error)
    {
        if (string.IsNullOrWhiteSpace(error))
            throw new DomainException("Last error cannot be empty.");

        var normalizedError = error.Trim();

        if (normalizedError.Length > MaxLastErrorLength)
            throw new DomainException($"Last error cannot exceed {MaxLastErrorLength} characters.");

        return normalizedError;
    }

    private static DateTimeOffset ValidateUtcDate(DateTimeOffset date, string parameterName)
    {
        if (date.Offset != TimeSpan.Zero)
            throw new DomainException($"{parameterName} must be in UTC.");

        return date;
    }

    private void EnsureNotPublished()
    {
        if (PublishedAt.HasValue)
            throw new DomainException("Published messages cannot be changed.");
    }

    private void IncrementAttemptCount()
    {
        if (AttemptCount == int.MaxValue)
            throw new DomainException("Attempt count has reached its maximum value.");

        AttemptCount++;
    }
}
