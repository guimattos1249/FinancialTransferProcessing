namespace FinancialTransferProcessing.Worker.Consumers.Enums;

// O dispatcher traduz esse resultado em uma ação de transporte. O handler
// continua independente de conceitos como ack, nack, exchange e fila.
internal enum ETransferRequestedDeliveryOutcome
{
    Processed,
    InvalidEnvelope
}
