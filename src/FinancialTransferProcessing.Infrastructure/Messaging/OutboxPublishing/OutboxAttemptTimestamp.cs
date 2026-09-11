using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Infrastructure.Messaging.OutboxPublishing;

internal static class OutboxAttemptTimestamp
{
    public static DateTimeOffset Resolve(OutboxMessage message)
    {
        var currentDateUtc = DateTimeOffset.UtcNow;

        return currentDateUtc < message.OccurredAt
            ? message.OccurredAt
            : currentDateUtc;
    }
}
