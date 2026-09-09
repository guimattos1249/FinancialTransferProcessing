using FinancialTransferProcessing.Application.Contracts;
using FinancialTransferProcessing.Application.Contracts.Messaging;
using FinancialTransferProcessing.Application.Contracts.Repositories.OutboxMessages;
using FinancialTransferProcessing.Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FinancialTransferProcessing.Infrastructure.Messaging.OutboxPublishing;

internal sealed class OutboxPublisherBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOutboxMessagePublisher messagePublisher,
    IOptions<OutboxPublisherOptions> options, 
    ILogger<OutboxPublisherBackgroundService> logger) : BackgroundService
{
    private readonly OutboxPublisherOptions _options = options.Value;
    private readonly IOutboxMessagePublisher _messagePublisher = messagePublisher;

    private sealed record PublicationResult(
        OutboxMessage Message,
        DateTimeOffset AttemptedAtUtc,
        Exception? Exception)
    {
        public bool IsSuccess => Exception is null;
    }

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

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var acquiredAtUtc = DateTimeOffset.UtcNow;
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

        var publicationTasks = messages.Select(
            message => PublishMessageAsync(
                message,
                cancellationToken));

        var publicationResults = await Task.WhenAll(publicationTasks);

        foreach (var result in publicationResults)
        {
            if (result.IsSuccess)
            {
                result.Message.MarkAsPublished(leaseId, result.AttemptedAtUtc);

                continue;
            }

            var retryDelay = CalculateRetryDelay(result.Message.AttemptCount);

            var nextAttemptAtUtc = result.AttemptedAtUtc.Add(retryDelay);

            result.Message.RegisterFailedAttempt(
                leaseId,
                FormatError(result.Exception!),
                result.AttemptedAtUtc,
                nextAttemptAtUtc);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);

        var publishedCount = 0;

        foreach (var result in publicationResults)
        {
            if (result.IsSuccess)
            {
                publishedCount++;

                logger.LogInformation(
                    "Outbox message {MessageId} published. LeaseId: {LeaseId}, Attempt: {AttemptCount}.",
                    result.Message.MessageId,
                    leaseId,
                    result.Message.AttemptCount);

                continue;
            }

            logger.LogWarning(
                result.Exception,
                "Outbox message {MessageId} failed.  LeaseId: {LeaseId}, Attempt: {AttemptCount}, NextAttempt: {NextAttemptAtUtc}.",
                result.Message.MessageId,
                leaseId,
                result.Message.AttemptCount,
                result.Message.NextAttemptAt);
        }

        var failedCount = publicationResults.Length - publishedCount;

        logger.LogInformation(
            "Outbox wave {LeaseId} completed. MessageCount: {MessageCount}, PublishedCount: {PublishedCount}, FailedCount: {FailedCount}.",
            leaseId,
            publicationResults.Length,
            publishedCount,
            failedCount);
    }

    private async Task<PublicationResult> PublishMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        try
        {
            await _messagePublisher.PublishAsync(message, cancellationToken);

            return new PublicationResult(message, DateTimeOffset.UtcNow, null);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new PublicationResult(message, DateTimeOffset.UtcNow, ex);
        }
    }

    private TimeSpan CalculateRetryDelay(
    int previousAttemptCount)
    {
        var delay = _options.InitialRetryDelay;
        var maximumDelay = _options.MaxRetryDelay;

        for (var attempt = 0;
             attempt < previousAttemptCount;
             attempt++)
        {
            if (delay >= maximumDelay)
                return maximumDelay;

            if (delay.Ticks > maximumDelay.Ticks / 2)
                return maximumDelay;

            delay = TimeSpan.FromTicks(delay.Ticks * 2);
        }

        return delay > maximumDelay
            ? maximumDelay
            : delay;
    }

    private static string FormatError(Exception exception)
    {
        var error = exception.ToString();

        return error.Length <= OutboxMessage.MaxLastErrorLength
            ? error
            : error[..OutboxMessage.MaxLastErrorLength];
    }
}
