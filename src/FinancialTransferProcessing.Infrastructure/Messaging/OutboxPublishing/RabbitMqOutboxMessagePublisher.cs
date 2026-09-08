using FinancialTransferProcessing.Application.Contracts.Messaging;
using FinancialTransferProcessing.Domain.Entities;
using RabbitMQ.Client;

namespace FinancialTransferProcessing.Infrastructure.Messaging.OutboxPublishing;

internal sealed class RabbitMqOutboxMessagePublisher(
    RabbitMqConnectionProvider connectionProvider) : IOutboxMessagePublisher
{
    public async Task PublishAsync(OutboxMessage message, CancellationToken cancellationToken = default)
    {
        var connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true);

        await using var channel = await connection.CreateChannelAsync(channelOptions, cancellationToken);
    }
}
