using FinancialTransferProcessing.Application.Contracts.Repositories.OutboxMessages;
using FinancialTransferProcessing.Domain.Entities;
using FinancialTransferProcessing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransferProcessing.Infrastructure.Repositories.OutboxMessages;

public sealed class OutboxMessageRepository(ApplicationDbContext context) : IOutboxMessageWriteOnlyRepository, IOutboxMessageReadOnlyRepository
{
    public async Task AddAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        await context.OutboxMessages.AddAsync(message, cancellationToken);
    }

    public async Task<IReadOnlyList<OutboxMessage>> GetPublishableBatchAsync(DateTimeOffset currentDateUtc, int batchSize, CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(
        batchSize,
        1);

        if (currentDateUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Current date must be in UTC.",
                nameof(currentDateUtc));
        }

        return await context.OutboxMessages
            .Where(message =>
                message.PublishedAt == null
                && (
                    message.NextAttemptAt == null
                    || message.NextAttemptAt <= currentDateUtc
                ))
            .OrderBy(message => message.OccurredAt)
            .ThenBy(message => message.MessageId)
            .Take(batchSize)
            .ToListAsync(cancellationToken);
    }
}
