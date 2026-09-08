using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Application.Contracts.Repositories.OutboxMessages;

public interface IOutboxMessageLeaseRepository
{
    Task<IReadOnlyList<OutboxMessage>> AcquirePublishableBatchAsync(
        Guid leaseId,
        DateTimeOffset acquiredAtUtc,
        DateTimeOffset leaseExpiresAtUtc,
        int batchSize,
        CancellationToken cancellationToken = default);
}
