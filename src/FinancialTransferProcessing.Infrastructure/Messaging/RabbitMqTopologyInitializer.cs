using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace FinancialTransferProcessing.Infrastructure.Messaging;

internal sealed class RabbitMqTopologyInitializer(
    RabbitMqConnectionProvider connectionProvider) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var connection = await connectionProvider.GetConnectionAsync(cancellationToken);

        await using var channel = await connection.CreateChannelAsync(cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqTopology.TransfersExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        var quorumQueueArgs = new Dictionary<string, object?>
        {
            ["x-queue-type"] = "quorum"
        };

        await channel.QueueDeclareAsync(queue: RabbitMqTopology.ProcessingQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: quorumQueueArgs,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(queue: RabbitMqTopology.ProcessingQueueName,
            exchange: RabbitMqTopology.TransfersExchangeName,
            routingKey: RabbitMqTopology.TransferRequestedRoutingKey,
            cancellationToken: cancellationToken);

    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
