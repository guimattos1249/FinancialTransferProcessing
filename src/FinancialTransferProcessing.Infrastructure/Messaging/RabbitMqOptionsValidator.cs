using Microsoft.Extensions.Options;

namespace FinancialTransferProcessing.Infrastructure.Messaging;

internal sealed class RabbitMqOptionsValidator : IValidateOptions<RabbitMqOptions>
{
    public ValidateOptionsResult Validate(string? name, RabbitMqOptions options)
    {
        var failures = new List<string>();

        if (string.IsNullOrWhiteSpace(options.HostName))
            failures.Add("RabbitMq:HostName must be configured");

        if (options.Port is < 1 or > 65535)
            failures.Add("RabbitMq:Port must be between 1 and 65535");

        if (string.IsNullOrWhiteSpace(options.VirtualHost))
            failures.Add("RabbitMq:VirtualHost must be configured");

        if (string.IsNullOrWhiteSpace(options.UserName))
            failures.Add("RabbitMq:UserName must be configured");

        if (string.IsNullOrWhiteSpace(options.Password))
            failures.Add("RabbitMq:Password must be configured");

        if (string.IsNullOrWhiteSpace(options.ClientProvidedName))
            failures.Add("RabbitMq:ClientProvidedName must be configured");

        var retryDelays = options.Retry?.Delays;

        if (retryDelays is null || retryDelays.Count == 0)
        {
            failures.Add("RabbitMq:Retry:Delays must contain at least one delay");
        }
        else
        {
            if (retryDelays.Any(delay => delay <= TimeSpan.Zero))
            {
                failures.Add(
                    "RabbitMq:Retry:Delays must contain only positive values");
            }

            if (retryDelays.Distinct().Count() != retryDelays.Count)
            {
                failures.Add(
                    "RabbitMq:Retry:Delays must not contain duplicate values");
            }

            if (!retryDelays.SequenceEqual(retryDelays.Order()))
            {
                failures.Add(
                    "RabbitMq:Retry:Delays must be ordered from shortest to longest");
            }
        }

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
