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

        return failures.Count > 0
            ? ValidateOptionsResult.Fail(failures)
            : ValidateOptionsResult.Success;
    }
}
