namespace FinancialTransferProcessing.Worker.Consumers;

public sealed class TransferConsumerOptions
{
    public const string SectionName = "TransferConsumer";
    public int PrefetchCount { get; set; }
    public int MaxDegreeOfParallelism { get; set; }
}
