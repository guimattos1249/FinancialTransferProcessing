using FinancialTransferProcessing.Worker.Consumers.RabbitMq;
using FinancialTransferProcessing.Worker.Consumers.Enums;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinancialTransferProcessing.Worker.Consumers;

// O dispatcher coordena exatamente uma entrega e contém a tabela de decisão:
// Processed       -> ack;
// InvalidEnvelope -> publicar na DLQ e depois ack;
// Exception       -> nack com requeue (política provisória da Task 14).
//
// Ele não abre conexão/canal e não conhece regras financeiras. Seu papel é
// traduzir o resultado do handler para uma ação do protocolo AMQP.
internal sealed class TransferRequestedDeliveryDispatcher(
    TransferRequestedDeliveryHandler deliveryHandler,
    RabbitMqDeliveryOperations deliveryOperations,
    ILogger<TransferRequestedDeliveryDispatcher> logger)
{
    private readonly TransferRequestedDeliveryHandler _deliveryHandler =
        deliveryHandler;

    private readonly RabbitMqDeliveryOperations _deliveryOperations =
        deliveryOperations;

    private readonly ILogger<TransferRequestedDeliveryDispatcher> _logger =
        logger;

    public async Task DispatchAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        CancellationToken stoppingToken)
    {
        // delivery.CancellationToken representa o ciclo da entrega/canal;
        // stoppingToken representa o ciclo do Worker. O token ligado cancela o
        // processamento quando qualquer um desses dois ciclos terminar.
        using var deliveryCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                stoppingToken,
                delivery.CancellationToken);

        var cancellationToken = deliveryCts.Token;

        try
        {
            var outcome =
                await _deliveryHandler.HandleAsync(
                    delivery.Body,
                    delivery.BasicProperties,
                    cancellationToken);

            switch (outcome)
            {
                case ETransferRequestedDeliveryOutcome.Processed:
                    // Chegar aqui significa que desserialização e Application
                    // terminaram sem exceção. Só então removemos o trabalho da fila.
                    await _deliveryOperations.AcknowledgeAsync(
                        channel,
                        delivery.DeliveryTag,
                        cancellationToken);

                    break;

                case ETransferRequestedDeliveryOutcome.InvalidEnvelope:
                    // Envelope inválido é permanente: reexecutá-lo não corrigirá
                    // JSON, tipo ou versão. Por isso ele recebe destino conhecido.
                    await _deliveryOperations
                        .SendToDeadLetterAndAcknowledgeAsync(
                            channel,
                            delivery,
                            cancellationToken);

                    break;

                default:
                    throw new InvalidOperationException(
                        $"Unsupported delivery outcome '{outcome}'.");
            }
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            // Não fazemos ack nem nack no cancelamento. Quando o canal fechar,
            // o RabbitMQ devolverá automaticamente a entrega não confirmada.
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "TransferRequested processing failed. MessageId: {MessageId}, CorrelationId: {CorrelationId}.",
                delivery.BasicProperties.MessageId,
                delivery.BasicProperties.CorrelationId);

            // Neste estágio qualquer exceção operacional é tratada como
            // transitória. A Task 17 adicionará classificação e retry com atraso.
            await _deliveryOperations.TryNegativeAcknowledgeAsync(
                channel,
                delivery.DeliveryTag,
                stoppingToken);
        }
    }
}
