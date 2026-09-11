using FinancialTransferProcessing.Application.Contracts;
using FinancialTransferProcessing.Application.Contracts.Messaging;
using FinancialTransferProcessing.Application.Contracts.Repositories.OutboxMessages;
using FinancialTransferProcessing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Diagnostics;

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
        TimeSpan Duration,
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
        var remainingMessages = _options.BatchSize;

        while (remainingMessages > 0)
        {
            var waveSize = Math.Min(
                remainingMessages,
                _options.MaxDegreeOfParallelism);

            var processedMessages = await ProcessWaveAsync(
                waveSize,
                cancellationToken);

            if (processedMessages == 0)
                break;

            remainingMessages -= processedMessages;
        }
    }

    private async Task<int> ProcessWaveAsync(
        int waveSize,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();

        var leaseRepository = scope.ServiceProvider.GetRequiredService<IOutboxMessageLeaseRepository>();

        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var acquiredAtUtc = DateTimeOffset.UtcNow;
        var leaseId = Guid.NewGuid();
        var leaseExpiresAtUtc = acquiredAtUtc.Add(_options.LeaseDuration);

        var messages = await leaseRepository.AcquirePublishableBatchAsync(
            leaseId,
            acquiredAtUtc,
            leaseExpiresAtUtc,
            waveSize,
            cancellationToken);

        if (messages.Count == 0)
            return 0;

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

            var retryDelay = OutboxRetryPolicy.CalculateDelay(
                result.Message.AttemptCount,
                _options.InitialRetryDelay,
                _options.MaxRetryDelay);

            var nextAttemptAtUtc = result.AttemptedAtUtc.Add(retryDelay);

            result.Message.RegisterFailedAttempt(
                leaseId,
                OutboxRetryPolicy.FormatError(result.Exception!),
                result.AttemptedAtUtc,
                nextAttemptAtUtc);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            logger.LogWarning(
                exception,
                "Outbox wave {LeaseId} was not persisted because lease ownership changed. MessageCount: {MessageCount}.",
                leaseId,
                publicationResults.Length);

            return 0;
        }

        OutboxPublisherMetrics.RecordWaveSize(publicationResults.Length);

        var publishedCount = 0;

        foreach (var result in publicationResults)
        {
            OutboxPublisherMetrics.RecordPublication(
                result.Message,
                result.IsSuccess,
                result.Duration);

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
                "Outbox message {MessageId} failed. LeaseId: {LeaseId}, Attempt: {AttemptCount}, NextAttempt: {NextAttemptAtUtc}.",
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

        return messages.Count;
    }

    private async Task<PublicationResult> PublishMessageAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var startedAtTimestamp = Stopwatch.GetTimestamp();

        try
        {
            await _messagePublisher.PublishAsync(message, cancellationToken);

            return new PublicationResult(
                message,
                OutboxAttemptTimestamp.Resolve(message),
                Stopwatch.GetElapsedTime(startedAtTimestamp),
                Exception: null);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return new PublicationResult(
                message,
                OutboxAttemptTimestamp.Resolve(message),
                Stopwatch.GetElapsedTime(startedAtTimestamp),
                Exception: ex);
        }
    }
}
