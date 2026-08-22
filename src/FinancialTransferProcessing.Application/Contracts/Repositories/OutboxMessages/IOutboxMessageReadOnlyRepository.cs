using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Application.Contracts.Repositories.OutboxMessages;

public interface IOutboxMessageReadOnlyRepository
{
    Task<IReadOnlyList<OutboxMessage>> GetPublishableBatchAsync(
        DateTimeOffset currentDate,
        int batchSize,
        CancellationToken cancellationToken = default);
}
