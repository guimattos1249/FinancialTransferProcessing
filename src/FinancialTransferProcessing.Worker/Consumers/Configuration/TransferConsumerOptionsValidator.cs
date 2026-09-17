using Microsoft.Extensions.Options;

namespace FinancialTransferProcessing.Worker.Consumers.Configuration;

// Valida no startup os limites usados pelo QoS e pelo dispatcher do RabbitMQ.
internal sealed class TransferConsumerOptionsValidator : IValidateOptions<TransferConsumerOptions>
{
    public ValidateOptionsResult Validate(string? name, TransferConsumerOptions options)
    {
        var failures = new List<string>();

        if (options.PrefetchCount is < 1)
            failures.Add("TransferConsumer:PrefetchCount must be a positive integer");

        if (options.MaxDegreeOfParallelism is < 1)
            failures.Add("TransferConsumer:MaxDegreeOfParallelism must be a positive integer");

        if (options.PrefetchCount > ushort.MaxValue)
            failures.Add("TransferConsumer:PrefetchCount cannot exceed 65535");

        if (options.MaxDegreeOfParallelism > options.PrefetchCount)
            failures.Add("TransferConsumer:MaxDegreeOfParallelism cannot exceed PrefetchCount");

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
