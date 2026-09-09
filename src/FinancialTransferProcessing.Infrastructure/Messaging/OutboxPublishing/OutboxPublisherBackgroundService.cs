using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinancialTransferProcessing.Infrastructure.Messaging.OutboxPublishing;

internal sealed class OutboxPublisherBackgroundService(
    IOptions<OutboxPublisherOptions> options, 
    ILogger<OutboxPublisherBackgroundService> logger) : BackgroundService
{
    private readonly OutboxPublisherOptions _options = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error occurred while processing outbox messages.");
            }

            try
            {
                await Task.Delay(_options.PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private static Task ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
