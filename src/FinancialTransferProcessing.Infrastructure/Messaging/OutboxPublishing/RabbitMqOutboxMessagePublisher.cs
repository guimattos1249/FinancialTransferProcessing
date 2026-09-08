using FinancialTransferProcessing.Application.Contracts.Messaging;
using FinancialTransferProcessing.Domain.Entities;
using RabbitMQ.Client;
using System.Text;

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

        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            ContentEncoding = "utf-8",
            MessageId = message.MessageId.ToString(),
            CorrelationId = message.CorrelationId.ToString(),
            Type = message.Type,
            Headers = new Dictionary<string, object?>
            {
                ["schema-version"] = message.SchemaVersion
            }
        };

        var body = Encoding.UTF8.GetBytes(message.Payload);

        await channel.BasicPublishAsync(
            exchange: RabbitMqTopology.TransfersExchangeName,
            routingKey: ResolveRoutingKey(message.Type),
            mandatory: true,
            basicProperties: properties,
            body: body,
            cancellationToken: cancellationToken);
    }

    private static string ResolveRoutingKey(string messageType)
    {
        return messageType switch
        {
            TransferRequested.MessageType =>
                RabbitMqTopology.TransferRequestedRoutingKey,

            _ => throw new InvalidOperationException(
                $"No RabbitMQ routing key is configured for message type '{messageType}'.")
        };
    }
}
