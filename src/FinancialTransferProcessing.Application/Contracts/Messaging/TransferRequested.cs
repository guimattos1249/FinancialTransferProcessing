namespace FinancialTransferProcessing.Application.Contracts.Messaging;

public sealed record TransferRequested
{
    public const string MessageType = "TransferRequested";
    public const int CurrentSchemaVersion = 1;

    public Guid MessageId { get; }
    public Guid TransferId { get; }
    public DateTimeOffset OccurredAt { get; }
    public string CorrelationId { get; }
    public int SchemaVersion { get; }

    public TransferRequested(
        Guid messageId,
        Guid transferId,
        DateTimeOffset occurredAt,
        string correlationId,
        int schemaVersion)
    {
        if (occurredAt.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "OccurredAt must be in UTC.",
                nameof(occurredAt));
        }

        if (messageId == Guid.Empty)
        {
            throw new ArgumentException(
                "Message ID cannot be empty.",
                nameof(messageId));
        }

        if (transferId == Guid.Empty)
        {
            throw new ArgumentException(
                "Transfer ID cannot be empty.",
                nameof(transferId));
        }

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new ArgumentException(
                "Correlation ID cannot be empty.",
                nameof(correlationId));
        }

        if (schemaVersion != CurrentSchemaVersion)
        {
            throw new ArgumentOutOfRangeException(
                nameof(schemaVersion),
                schemaVersion,
                $"Schema version must be {CurrentSchemaVersion}.");
        }

        MessageId = messageId;
        TransferId = transferId;
        OccurredAt = occurredAt;
        CorrelationId = correlationId.Trim();
        SchemaVersion = schemaVersion;
    }
}
