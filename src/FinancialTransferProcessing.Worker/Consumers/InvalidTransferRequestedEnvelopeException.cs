namespace FinancialTransferProcessing.Worker.Consumers;

// Diferencia um envelope permanentemente inválido de uma falha transitória
// ocorrida enquanto o caso de uso processa uma mensagem válida.
internal sealed class InvalidTransferRequestedEnvelopeException : Exception
{
    public InvalidTransferRequestedEnvelopeException(string message)
        : base(message)
    {
    }

    public InvalidTransferRequestedEnvelopeException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
