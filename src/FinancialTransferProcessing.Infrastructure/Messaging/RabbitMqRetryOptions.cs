namespace FinancialTransferProcessing.Infrastructure.Messaging;

public sealed class RabbitMqRetryOptions
{
    public List<TimeSpan> Delays { get; set; } = [];
}
