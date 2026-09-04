namespace FinancialTransferProcessing.Infrastructure.Messaging;

internal static class RabbitMqTopology
{
    public const string TransfersExchangeName = "financial-transfers";
    public const string RetryExchangeName = "financial-transfers.retry";
    public const string DeadLetterExchangeName = "financial-transfers.dead-letter";

    public const string TransferRequestedRoutingKey = "transfer.requested";
    private const string RetryRoutingKeyPrefix = "transfer.requested.retry";
    public const string DeadLetterRoutingKey = "transfer.requested.dead-letter";

    public const string ProcessingQueueName = "transfer-processing";
    private const string RetryQueueNamePrefix = "transfer-processing.retry";
    public const string DeadLetterQueueName = "transfer-processing.dlq";

    public static string GetRetryQueueName(TimeSpan delay)
    {
        return $"{RetryQueueNamePrefix}.{GetDelayToken(delay)}";
    }

    public static string GetRetryRoutingKey(TimeSpan delay)
    {
        return $"{RetryRoutingKeyPrefix}.{GetDelayToken(delay)}";
    }

    private static string GetDelayToken(TimeSpan delay)
    {
        var milliseconds =
            delay.Ticks / TimeSpan.TicksPerMillisecond;

        return $"{milliseconds}ms";
    }
}
