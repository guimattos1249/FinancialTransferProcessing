namespace FinancialTransferProcessing.Infrastructure.Messaging.OutboxPublishing;

public sealed class OutboxPublisherOptions
{
    public const string SectionName = "OutboxPublisher";

    public TimeSpan PollingInterval { get; set; }
    public int BatchSize { get; set; }
    public int MaxDegreeOfParallelism { get; set; }
    public TimeSpan LeaseDuration { get; set; }
    public TimeSpan PublishTimeout { get; set; }
    public TimeSpan InitialRetryDelay { get; set; }
    public TimeSpan MaxRetryDelay { get; set; }
}
