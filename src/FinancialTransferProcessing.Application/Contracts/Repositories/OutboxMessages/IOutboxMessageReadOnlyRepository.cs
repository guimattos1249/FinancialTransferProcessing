using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Application.Contracts.Repositories.OutboxMessages;

public interface IOutboxMessageReadOnlyRepository
{
    Task<IReadOnlyList<OutboxMessage>> GetPublishableBatchAsync(
        DateTimeOffset currentDateUtc,
        int batchSize,
        CancellationToken cancellationToken = default);
}
