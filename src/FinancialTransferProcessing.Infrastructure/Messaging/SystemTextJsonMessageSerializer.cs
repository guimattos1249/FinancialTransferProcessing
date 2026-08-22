using FinancialTransferProcessing.Application.Contracts.Messaging;
using System.Text.Json;

namespace FinancialTransferProcessing.Infrastructure.Messaging;

internal sealed class SystemTextJsonMessageSerializer : IMessageSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented = false
        };

    public string Serialize<TMessage>(TMessage message)
    {
        ArgumentNullException.ThrowIfNull(message);

        return JsonSerializer.Serialize(message, SerializerOptions);
    }

    public TMessage Deserialize<TMessage>(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            throw new ArgumentException(
                "Payload cannot be empty.",
                nameof(payload));

        return JsonSerializer.Deserialize<TMessage>(
            payload,
            SerializerOptions)
            ?? throw new JsonException(
                $"Could not deserialize payload as {typeof(TMessage).Name}.");
    }
}
