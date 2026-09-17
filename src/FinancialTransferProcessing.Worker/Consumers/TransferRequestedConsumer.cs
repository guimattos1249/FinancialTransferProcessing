using FinancialTransferProcessing.Infrastructure.Messaging;
using FinancialTransferProcessing.Worker.Consumers.Configuration;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FinancialTransferProcessing.Worker.Consumers;

// Este BackgroundService controla somente o ciclo de vida do consumidor.
// ExecuteAsync roda uma vez, abre um canal duradouro e registra uma callback.
// O RabbitMQ chama essa callback para cada entrega; não existe polling aqui.
//
// Fluxo deste arquivo:
// conexão -> canal -> QoS -> BasicConsume -> aguardar shutdown -> BasicCancel.
// O conteúdo de cada entrega é delegado para outro componente.
internal sealed class TransferRequestedConsumer(
    RabbitMqConnectionProvider connectionProvider,
    TransferRequestedDeliveryDispatcher deliveryDispatcher,
    IOptions<TransferConsumerOptions> options,
    ILogger<TransferRequestedConsumer> logger) : BackgroundService
{
    private readonly RabbitMqConnectionProvider _connectionProvider =
        connectionProvider;

    // O dispatcher possui o fluxo de uma mensagem. Separá-lo mantém este
    // BackgroundService focado em inicialização e encerramento do transporte.
    private readonly TransferRequestedDeliveryDispatcher _deliveryDispatcher =
        deliveryDispatcher;

    // IOptions<T> é o wrapper do sistema de configuração. O consumidor usa o
    // Value validado no startup para acessar diretamente as duas propriedades.
    private readonly TransferConsumerOptions _options = options.Value;

    private readonly ILogger<TransferRequestedConsumer> _logger = logger;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        // A conexão pertence ao provider singleton e é reutilizada durante a
        // vida do processo. O canal abaixo é exclusivo deste consumidor.
        var connection =
            await _connectionProvider.GetConnectionAsync(stoppingToken);

        // publisherConfirmationsEnabled faz BasicPublishAsync aguardar a
        // confirmação do broker. Sem isso, poderíamos dar ack na mensagem
        // original antes de saber se sua cópia realmente chegou à DLQ.
        //
        // consumerDispatchConcurrency define quantas callbacks o client pode
        // executar simultaneamente. Isso não altera o prefetch do broker.
        var channelOptions = new CreateChannelOptions(
            publisherConfirmationsEnabled: true,
            publisherConfirmationTrackingEnabled: true,
            consumerDispatchConcurrency:
                checked((ushort)_options.MaxDegreeOfParallelism));

        // O await using mantém um único canal aberto durante todo o consumo e
        // garante seu descarte quando ExecuteAsync terminar.
        await using var channel =
            await connection.CreateChannelAsync(
                channelOptions,
                stoppingToken);

        // prefetchSize=0 desabilita limite por bytes. prefetchCount limita a
        // quantidade de mensagens entregues que ainda não receberam ack/nack.
        // global=false aplica o limite ao consumidor, não ao canal inteiro.
        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount:
                checked((ushort)_options.PrefetchCount),
            global: false,
            cancellationToken: stoppingToken);

        // AsyncEventingBasicConsumer permite que o client aguarde a Task da
        // callback, em vez de tratar o processamento como fire-and-forget.
        var consumer = new AsyncEventingBasicConsumer(channel);

        // O buffer de Body só é garantido enquanto a callback estiver rodando.
        // Retornar a Task do dispatcher mantém a callback aberta até terminar
        // validação, Application e ack/nack/DLQ.
        consumer.ReceivedAsync += (_, delivery) =>
            _deliveryDispatcher.DispatchAsync(
                channel,
                delivery,
                stoppingToken);

        // autoAck=false é a base da entrega confiável: receber a mensagem não
        // remove seu trabalho da fila. Somente nosso BasicAck fará isso.
        // consumerTag vazio pede ao broker que gere um identificador único.
        var consumerTag =
            await channel.BasicConsumeAsync(
                queue: RabbitMqTopology.ProcessingQueueName,
                autoAck: false,
                consumerTag: string.Empty,
                noLocal: false,
                exclusive: false,
                arguments: null,
                consumer: consumer,
                cancellationToken: stoppingToken);

        _logger.LogInformation(
            "TransferRequested consumer started. Queue: {Queue}, PrefetchCount: {PrefetchCount}, MaxDegreeOfParallelism: {MaxDegreeOfParallelism}.",
            RabbitMqTopology.ProcessingQueueName,
            _options.PrefetchCount,
            _options.MaxDegreeOfParallelism);

        try
        {
            // Depois de BasicConsumeAsync, o trabalho chega por callbacks.
            // Esta espera impede que ExecuteAsync termine e descarte o canal.
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Cancelamento do Host é um encerramento normal, não uma falha.
        }
        finally
        {
            await CancelConsumerAsync(
                channel,
                consumerTag);
        }
    }

    private async Task CancelConsumerAsync(
        IChannel channel,
        string consumerTag)
    {
        if (!channel.IsOpen)
            return;

        try
        {
            // BasicCancel impede novas entregas para este consumidor. Usamos
            // CancellationToken.None porque o token do Host já foi cancelado;
            // reutilizá-lo impediria até a tentativa de avisar o broker.
            await channel.BasicCancelAsync(
                consumerTag,
                noWait: false,
                cancellationToken: CancellationToken.None);
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Failed to cancel TransferRequested consumer cleanly.");
        }
    }
}
