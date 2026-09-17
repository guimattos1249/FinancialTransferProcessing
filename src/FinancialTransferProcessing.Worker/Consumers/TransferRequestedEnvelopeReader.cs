using FinancialTransferProcessing.Application.Contracts.Messaging;
using FinancialTransferProcessing.Application.UseCases.Transfers.ProcessTransfer;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace FinancialTransferProcessing.Worker.Consumers;

internal sealed class TransferRequestedEnvelopeReader(IMessageSerializer serializer)
{
    private const string ExpectedContentType = "application/json";
    private const string ExpectedContentEncoding = "utf-8";
    private const string SchemaVersionHeader = "schema-version";

    private static readonly Encoding StrictUtf8 =
        new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: false,
            throwOnInvalidBytes: true);

    private readonly IMessageSerializer _serializer =
        serializer ?? throw new ArgumentNullException(nameof(serializer));

    public ProcessTransferRequest Read(
        ReadOnlyMemory<byte> body,
        IReadOnlyBasicProperties properties)
    {
        ArgumentNullException.ThrowIfNull(properties);

        ValidateContentType(properties.ContentType);
        ValidateContentEncoding(properties.ContentEncoding);
        ValidateMessageType(properties.Type);

        var propertyMessageId = ParseMessageId(properties.MessageId);
        var propertyCorrelationId =
            ValidateCorrelationId(properties.CorrelationId);

        var headerSchemaVersion = ReadSchemaVersion(properties);

        if (body.IsEmpty)
        {
            throw new InvalidTransferRequestedEnvelopeException(
                "The message body cannot be empty.");
        }

        var payload = DecodeBody(body);
        var message = Deserialize(payload);

        ValidateConsistency(
            message,
            propertyMessageId,
            propertyCorrelationId,
            headerSchemaVersion);

        return new ProcessTransferRequest(
            message.MessageId,
            message.TransferId,
            message.OccurredAt,
            message.CorrelationId);
    }

    private static void ValidateContentType(string? contentType)
    {
        if (!string.Equals(
            contentType,
            ExpectedContentType,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidTransferRequestedEnvelopeException(
                $"The message content type must be '{ExpectedContentType}'.");
        }
    }

    private static void ValidateContentEncoding(string? contentEncoding)
    {
        if (!string.Equals(
            contentEncoding,
            ExpectedContentEncoding,
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidTransferRequestedEnvelopeException(
                $"The message content encoding must be '{ExpectedContentEncoding}'.");
        }
    }

    private static void ValidateMessageType(string? messageType)
    {
        if (!string.Equals(
            messageType,
            TransferRequested.MessageType,
            StringComparison.Ordinal))
        {
            throw new InvalidTransferRequestedEnvelopeException(
                $"The message type must be '{TransferRequested.MessageType}'.");
        }
    }

    private static Guid ParseMessageId(string? messageId)
    {
        if (!Guid.TryParse(messageId, out var parsedMessageId) ||
            parsedMessageId == Guid.Empty)
        {
            throw new InvalidTransferRequestedEnvelopeException(
                "The message ID must be a non-empty GUID.");
        }

        return parsedMessageId;
    }

    private static string ValidateCorrelationId(string? correlationId)
    {
        if (string.IsNullOrWhiteSpace(correlationId))
        {
            throw new InvalidTransferRequestedEnvelopeException(
                "The correlation ID cannot be empty.");
        }

        return correlationId;
    }

    private static int ReadSchemaVersion(
        IReadOnlyBasicProperties properties)
    {
        if (properties.Headers is null ||
            !properties.Headers.TryGetValue(
                SchemaVersionHeader,
                out var headerValue) ||
            headerValue is not int schemaVersion)
        {
            throw new InvalidTransferRequestedEnvelopeException(
                $"The '{SchemaVersionHeader}' header must be an integer.");
        }

        if (schemaVersion != TransferRequested.CurrentSchemaVersion)
        {
            throw new InvalidTransferRequestedEnvelopeException(
                $"Schema version '{schemaVersion}' is not supported.");
        }

        return schemaVersion;
    }

    private static string DecodeBody(ReadOnlyMemory<byte> body)
    {
        try
        {
            return StrictUtf8.GetString(body.Span);
        }
        catch (DecoderFallbackException exception)
        {
            throw new InvalidTransferRequestedEnvelopeException(
                "The message body is not valid UTF-8.",
                exception);
        }
    }

    private TransferRequested Deserialize(string payload)
    {
        try
        {
            return _serializer.Deserialize<TransferRequested>(payload);
        }
        catch (Exception exception)
            when (exception is JsonException
                or ArgumentException
                or NotSupportedException)
        {
            throw new InvalidTransferRequestedEnvelopeException(
                "The message body is not a valid TransferRequested payload.",
                exception);
        }
    }

    private static void ValidateConsistency(
        TransferRequested message,
        Guid propertyMessageId,
        string propertyCorrelationId,
        int headerSchemaVersion)
    {
        if (message.MessageId != propertyMessageId)
        {
            throw new InvalidTransferRequestedEnvelopeException(
                "The message ID does not match the payload.");
        }

        if (!string.Equals(
            message.CorrelationId,
            propertyCorrelationId,
            StringComparison.Ordinal))
        {
            throw new InvalidTransferRequestedEnvelopeException(
                "The correlation ID does not match the payload.");
        }

        if (message.SchemaVersion != headerSchemaVersion)
        {
            throw new InvalidTransferRequestedEnvelopeException(
                "The schema version does not match the payload.");
        }
    }
}
