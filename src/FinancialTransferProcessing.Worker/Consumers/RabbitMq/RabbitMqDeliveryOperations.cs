using FinancialTransferProcessing.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinancialTransferProcessing.Worker.Consumers.RabbitMq;

// As callbacks processam mensagens em paralelo, mas compartilham o mesmo canal.
// Esta classe usa um SemaphoreSlim como mutex assíncrono para que comandos AMQP
// de callbacks diferentes não sejam escritos simultaneamente no IChannel.
//
// Importante: o lock não envolve validação, banco ou Application. Ele protege
// apenas publish/ack/nack, preservando o paralelismo do trabalho mais demorado.
internal sealed class RabbitMqDeliveryOperations(
    ILogger<RabbitMqDeliveryOperations> logger) : IDisposable
{
    private const string DeadLetterReasonHeader =
        "dead-letter-reason";

    private const string InvalidEnvelopeReason =
        "invalid-envelope";

    private readonly ILogger<RabbitMqDeliveryOperations> _logger = logger;
    private readonly SemaphoreSlim _channelLock = new(1, 1);

    public async Task AcknowledgeAsync(
        IChannel channel,
        ulong deliveryTag,
        CancellationToken cancellationToken)
    {
        // WaitAsync não bloqueia uma thread enquanto outra callback usa o canal.
        await _channelLock.WaitAsync(cancellationToken);

        try
        {
            // DeliveryTag identifica uma entrega dentro deste canal.
            // multiple:false confirma somente essa tag. Com multiple:true,
            // poderíamos confirmar por engano callbacks anteriores em andamento.
            await channel.BasicAckAsync(
                deliveryTag: deliveryTag,
                multiple: false,
                cancellationToken: cancellationToken);
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public async Task SendToDeadLetterAndAcknowledgeAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken)
    {
        // Publish e ack ficam dentro da mesma seção crítica para impedir que
        // outra callback intercale comandos entre essas duas operações.
        await _channelLock.WaitAsync(cancellationToken);

        try
        {
            var deadLetterProperties =
                CreateDeadLetterProperties(
                    delivery.BasicProperties);

            // mandatory=true exige que exista uma rota para a publicação.
            // Publisher confirms fazem o await representar a aceitação pelo
            // broker, não apenas a escrita local no socket.
            await channel.BasicPublishAsync(
                exchange:
                    RabbitMqTopology.DeadLetterExchangeName,
                routingKey:
                    RabbitMqTopology.DeadLetterRoutingKey,
                mandatory: true,
                basicProperties: deadLetterProperties,
                body: delivery.Body,
                cancellationToken: cancellationToken);

            // Somente depois da confirmação da publicação removemos a mensagem
            // original da fila de processamento. Se o processo cair depois do
            // publish e antes deste ack, poderá existir duplicata na DLQ; isso é
            // preferível a perder a mensagem e é esperado em entrega at-least-once.
            await channel.BasicAckAsync(
                deliveryTag: delivery.DeliveryTag,
                multiple: false,
                cancellationToken: cancellationToken);

            _logger.LogWarning(
                "Invalid TransferRequested sent to dead-letter queue. MessageId: {MessageId}, CorrelationId: {CorrelationId}.",
                delivery.BasicProperties.MessageId,
                delivery.BasicProperties.CorrelationId);
        }
        finally
        {
            _channelLock.Release();
        }
    }

    public async Task TryNegativeAcknowledgeAsync(
        IChannel channel,
        ulong deliveryTag,
        CancellationToken stoppingToken)
    {
        if (stoppingToken.IsCancellationRequested)
            return;

        try
        {
            await _channelLock.WaitAsync(stoppingToken);

            try
            {
                // requeue=true devolve a mesma entrega para processamento.
                // Isso pode criar loop rápido enquanto a falha persistir; a
                // Task 17 substituirá por retry progressivo e limitado.
                await channel.BasicNackAsync(
                    deliveryTag: deliveryTag,
                    multiple: false,
                    requeue: true,
                    cancellationToken: stoppingToken);
            }
            finally
            {
                _channelLock.Release();
            }
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Fechar o canal já devolve ao broker uma entrega sem confirmação.
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to negatively acknowledge delivery {DeliveryTag}.",
                deliveryTag);
        }
    }

    private static BasicProperties CreateDeadLetterProperties(
        IReadOnlyBasicProperties originalProperties)
    {
        // Headers recebidos pertencem à entrega original. Copiá-los impede que
        // a inclusão do motivo altere o objeto fornecido pelo RabbitMQ client.
        var headers =
            originalProperties.Headers is null
                ? new Dictionary<string, object?>()
                : new Dictionary<string, object?>(
                    originalProperties.Headers);

        headers[DeadLetterReasonHeader] =
            InvalidEnvelopeReason;

        // O construtor copia MessageId, CorrelationId, Type, ContentType e os
        // demais metadados. Persistent=true solicita armazenamento durável na
        // fila quorum, sujeito às garantias do próprio broker.
        return new BasicProperties(originalProperties)
        {
            Persistent = true,
            Headers = headers
        };
    }

    public void Dispose()
    {
        // Quando a instância for registrada no DI, o container chamará Dispose
        // no encerramento e liberará o handle interno do SemaphoreSlim.
        _channelLock.Dispose();
    }
}
