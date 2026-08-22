namespace FinancialTransferProcessing.Application.Contracts.Messaging;

public interface IMessageSerializer
{
    string Serialize<TMessage>(TMessage message);

    TMessage Deserialize<TMessage>(string payload);
}
