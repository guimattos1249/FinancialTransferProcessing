using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Infrastructure.Messaging.OutboxPublishing;

internal static class OutboxRetryPolicy
{
    public static TimeSpan CalculateDelay(
        int previousAttemptCount,
        TimeSpan initialDelay,
        TimeSpan maximumDelay)
    {
        var delay = initialDelay;

        for (var attempt = 0;
             attempt < previousAttemptCount;
             attempt++)
        {
            if (delay >= maximumDelay)
                return maximumDelay;

            if (delay.Ticks > maximumDelay.Ticks / 2)
                return maximumDelay;

            delay = TimeSpan.FromTicks(delay.Ticks * 2);
        }

        return delay > maximumDelay
            ? maximumDelay
            : delay;
    }

    public static string FormatError(Exception exception)
    {
        var error = exception.ToString();

        return error.Length <= OutboxMessage.MaxLastErrorLength
            ? error
            : error[..OutboxMessage.MaxLastErrorLength];
    }
}
