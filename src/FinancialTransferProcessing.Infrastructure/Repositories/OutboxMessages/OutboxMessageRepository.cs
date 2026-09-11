using FinancialTransferProcessing.Application.Contracts.Repositories.OutboxMessages;
using FinancialTransferProcessing.Domain.Entities;
using FinancialTransferProcessing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransferProcessing.Infrastructure.Repositories.OutboxMessages;

public sealed class OutboxMessageRepository(ApplicationDbContext context) : IOutboxMessageWriteOnlyRepository, IOutboxMessageReadOnlyRepository, IOutboxMessageLeaseRepository
{
    public async Task<IReadOnlyList<OutboxMessage>> AcquirePublishableBatchAsync(Guid leaseId, DateTimeOffset acquiredAtUtc, DateTimeOffset leaseExpiresAtUtc, int batchSize, CancellationToken cancellationToken = default)
    {
        if (leaseId == Guid.Empty)
        {
            throw new ArgumentException(
                "Lease Id cannot be empty.", 
                nameof(leaseId));
        }

        if (acquiredAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Acquisition date must be in UTC.",
                nameof(acquiredAtUtc));
        }

        if (leaseExpiresAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "Lease expiration date must be in UTC.",
                nameof(leaseExpiresAtUtc));
        }

        if (leaseExpiresAtUtc <= acquiredAtUtc)
            throw new ArgumentException(
                "Lease expiration date must be later than acquisition date.",
                nameof(leaseExpiresAtUtc));

        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

        // Usa um Raw SQL query para adquirir a batch de mensagens publicáveis com base nos critérios fornecidos.
        // Pega mensagens que não foram publicadas ou expiradas.
        // FOR UPDATE SKIP LOCKED faz o lock nessas mensagens, mas permite que outras transações adquiram outras mensagens sem esperar.
        var messages = await context.OutboxMessages
            .FromSqlInterpolated($"""
                SELECT * FROM outbox_messages 
                WHERE published_at IS NULL
                  AND (
                    next_attempt_at IS NULL
                    OR next_attempt_at <= {acquiredAtUtc}
                  )
                  AND (
                    lease_expires_at IS NULL
                    OR lease_expires_at <= {acquiredAtUtc}
                  )
                  ORDER BY occurred_at, message_id
                  FOR UPDATE SKIP LOCKED
                  LIMIT {batchSize}
            """)
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.AcquireLease(leaseId, acquiredAtUtc, leaseExpiresAtUtc);
        }

        await context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return messages;
    }

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
