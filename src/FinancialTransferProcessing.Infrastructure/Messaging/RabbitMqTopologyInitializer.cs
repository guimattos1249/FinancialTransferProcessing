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

        await DeclareExchangesAsync(channel, cancellationToken);
        await DeclareQueuesAsync(channel, cancellationToken);
        await DeclareBindingsAsync(channel, cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static async Task DeclareExchangesAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqTopology.TransfersExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqTopology.RetryExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);

        await channel.ExchangeDeclareAsync(
            exchange: RabbitMqTopology.DeadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
    }

    private static async Task DeclareQueuesAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        var quorumQueueArguments = new Dictionary<string, object?>
        {
            ["x-queue-type"] = "quorum"
        };

        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.ProcessingQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: quorumQueueArguments,
            cancellationToken: cancellationToken);

        await channel.QueueDeclareAsync(
            queue: RabbitMqTopology.DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: quorumQueueArguments,
            cancellationToken: cancellationToken);
    }

    private static async Task DeclareBindingsAsync(
        IChannel channel,
        CancellationToken cancellationToken)
    {
        await channel.QueueBindAsync(
            queue: RabbitMqTopology.ProcessingQueueName,
            exchange: RabbitMqTopology.TransfersExchangeName,
            routingKey: RabbitMqTopology.TransferRequestedRoutingKey,
            cancellationToken: cancellationToken);

        await channel.QueueBindAsync(
            queue: RabbitMqTopology.DeadLetterQueueName,
            exchange: RabbitMqTopology.DeadLetterExchangeName,
            routingKey: RabbitMqTopology.DeadLetterRoutingKey,
            cancellationToken: cancellationToken);
    }
}
