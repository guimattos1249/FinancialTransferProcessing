using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Application.Contracts.Repositories.OutboxMessages;

public interface IOutboxMessageWriteOnlyRepository
{
    Task AddAsync(OutboxMessage message, CancellationToken cancellationToken);
}
