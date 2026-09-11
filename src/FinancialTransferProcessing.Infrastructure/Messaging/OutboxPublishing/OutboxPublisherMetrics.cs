using FinancialTransferProcessing.Domain.Entities;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace FinancialTransferProcessing.Infrastructure.Messaging.OutboxPublishing;

internal static class OutboxPublisherMetrics
{
    private static readonly Meter PublisherMeter =
        new("FinancialTransferProcessing.OutboxPublishing");

    private static readonly Counter<long> PublicationAttempts =
        PublisherMeter.CreateCounter<long>(
            "outbox.publisher.attempts",
            unit: "{message}");

    private static readonly Histogram<double> PublicationDuration =
        PublisherMeter.CreateHistogram<double>(
            "outbox.publisher.duration",
            unit: "ms");

    private static readonly Histogram<int> WaveSize =
        PublisherMeter.CreateHistogram<int>(
            "outbox.publisher.wave.size",
            unit: "{message}");

    public static void RecordWaveSize(int messageCount)
    {
        WaveSize.Record(messageCount);
    }

    public static void RecordPublication(
        OutboxMessage message,
        bool isSuccess,
        TimeSpan duration)
    {
        var tags = new TagList
        {
            { "message.type", message.Type },
            { "message.schema_version", message.SchemaVersion },
            { "result", isSuccess ? "published" : "failed" }
        };

        PublicationAttempts.Add(1, tags);
        PublicationDuration.Record(duration.TotalMilliseconds, tags);
    }
}
