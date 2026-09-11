using Microsoft.Extensions.Options;

namespace FinancialTransferProcessing.Infrastructure.Messaging.OutboxPublishing;

internal sealed class OutboxPublisherOptionsValidator : IValidateOptions<OutboxPublisherOptions>
{
    public ValidateOptionsResult Validate(string? name, OutboxPublisherOptions options)
    {
        var failures = new List<string>();

        if (options.PollingInterval <= TimeSpan.Zero)
        {
            failures.Add(
                "OutboxPublisher:PollingInterval must be greater than zero");
        }

        if (options.BatchSize <= 0)
        {
            failures.Add(
                "OutboxPublisher:BatchSize must be greater than zero");
        }

        if (options.MaxDegreeOfParallelism <= 0)
        {
            failures.Add(
                "OutboxPublisher:MaxDegreeOfParallelism must be greater than zero");
        }

        if (options.MaxDegreeOfParallelism > options.BatchSize)
        {
            failures.Add(
                "OutboxPublisher:MaxDegreeOfParallelism cannot exceed BatchSize");
        }

        if (options.PublishTimeout <= TimeSpan.Zero)
        {
            failures.Add(
                "OutboxPublisher:PublishTimeout must be greater than zero");
        }

        if (options.LeaseDuration <= options.PublishTimeout)
        {
            failures.Add(
                "OutboxPublisher:LeaseDuration must be greater than PublishTimeout");
        }

        if (options.InitialRetryDelay <= TimeSpan.Zero)
        {
            failures.Add(
                "OutboxPublisher:InitialRetryDelay must be greater than zero");
        }

        if (options.MaxRetryDelay < options.InitialRetryDelay)
        {
            failures.Add(
                "OutboxPublisher:MaxRetryDelay must be greater than or equal to InitialRetryDelay");
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}