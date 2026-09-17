using FinancialTransferProcessing.Application.UseCases.Transfers.ProcessTransfer;
using FinancialTransferProcessing.Worker.Consumers.Enums;
using RabbitMQ.Client;

namespace FinancialTransferProcessing.Worker.Consumers;

// Faz a ponte entre uma entrega já recebida e o caso de uso da Application.
// Ack, nack e dead-letter permanecem fora daqui por serem decisões de transporte.
internal sealed class TransferRequestedDeliveryHandler(
    TransferRequestedEnvelopeReader envelopeReader,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<TransferRequestedDeliveryHandler> logger)
{
    private readonly TransferRequestedEnvelopeReader _envelopeReader = envelopeReader;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;
    private readonly ILogger<TransferRequestedDeliveryHandler> _logger = logger;

    public async Task<ETransferRequestedDeliveryOutcome> HandleAsync(
        ReadOnlyMemory<byte> body,
        IReadOnlyBasicProperties properties,
        CancellationToken cancellationToken)
    {
        ProcessTransferRequest request;

        try
        {
            request = _envelopeReader.Read(body, properties);
        }
        catch (InvalidTransferRequestedEnvelopeException exception)
        {
            _logger.LogWarning(
                exception,
                "Invalid TransferRequested envelope received. MessageId: {MessageId}, CorrelationId: {CorrelationId}.",
                properties.MessageId,
                properties.CorrelationId);

            return ETransferRequestedDeliveryOutcome.InvalidEnvelope;
        }

        // Um escopo por entrega impede o compartilhamento de DbContext e de
        // outros serviços scoped entre callbacks executados em paralelo.
        await using var scope = _serviceScopeFactory.CreateAsyncScope();

        var processor = scope.ServiceProvider.GetRequiredService<IProcessTransferUseCase>();

        var response = await processor.Execute(request, cancellationToken);

        _logger.LogInformation(
            "TransferRequested processed. MessageId: {MessageId}, TransferId: {TransferId}, Status: {Status}",
            request.MessageId,
            response.TransferId,
            response.Status);

        return ETransferRequestedDeliveryOutcome.Processed;
    }
}
