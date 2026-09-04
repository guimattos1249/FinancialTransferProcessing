namespace FinancialTransferProcessing.Infrastructure.Messaging;

internal static class RabbitMqTopology
{
    public const string TransfersExchangeName = "financial-transfers";
    public const string RetryExchangeName = "financial-transfers.retry";
    public const string DeadLetterExchangeName = "financial-transfers.dead-letter";

    public const string TransferRequestedRoutingKey = "transfer.requested";
    public const string RetryRoutingKey = "transfer.requested.retry";
    public const string DeadLetterRoutingKey = "transfer.requested.dead-letter";

    public const string ProcessingQueueName = "transfer-processing";
    public const string RetryQueueNamePrefix = "transfer-processing.retry";
    public const string DeadLetterQueueName = "transfer-processing.dlq";
}
