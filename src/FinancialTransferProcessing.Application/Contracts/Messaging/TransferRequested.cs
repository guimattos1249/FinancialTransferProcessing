namespace FinancialTransferProcessing.Application.Contracts.Messaging;

public sealed record TransferRequested(
    Guid MessageId,
    Guid TransferId,
    DateTimeOffset OcurredAt,
    string CorrelationId,
    int SchemaVersion)
{
    public const string MessageType = "TransferRequested";
    public const int CurrentSchemaVersion = 1;
}
