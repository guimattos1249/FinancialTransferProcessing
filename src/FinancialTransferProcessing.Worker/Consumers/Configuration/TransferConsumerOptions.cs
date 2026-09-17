namespace FinancialTransferProcessing.Worker.Consumers.Configuration;

// Prefetch controla quantas mensagens podem ficar sem ack no processo.
// MaxDegreeOfParallelism controla quantos callbacks executam simultaneamente.
public sealed class TransferConsumerOptions
{
    public const string SectionName = "TransferConsumer";
    public int PrefetchCount { get; set; }
    public int MaxDegreeOfParallelism { get; set; }
}
