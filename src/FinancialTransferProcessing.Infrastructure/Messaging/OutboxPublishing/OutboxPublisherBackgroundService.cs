using FinancialTransferProcessing.Application.Contracts.Repositories.OutboxMessages;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinancialTransferProcessing.Infrastructure.Messaging.OutboxPublishing;

internal sealed class OutboxPublisherBackgroundService(
    IServiceScopeFactory scopeFactory,
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

    private async Task ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var leaseRepository = scope.ServiceProvider.GetRequiredService<IOutboxMessageLeaseRepository>();

        var acquiredAtUtc = DateTime.UtcNow;
        var leaseId = Guid.NewGuid();
        var leaseExpiresAtUtc = acquiredAtUtc.Add(_options.LeaseDuration);

        var waveSize = Math.Min(
            _options.BatchSize,
            _options.MaxDegreeOfParallelism);

        var messages = await leaseRepository.AcquirePublishableBatchAsync(
            leaseId,
            acquiredAtUtc,
            leaseExpiresAtUtc,
            waveSize,
            cancellationToken);

        if (messages.Count == 0)
            return;
    }
}
