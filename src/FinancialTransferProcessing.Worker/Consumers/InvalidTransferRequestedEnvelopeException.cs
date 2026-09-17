namespace FinancialTransferProcessing.Worker.Consumers;

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
