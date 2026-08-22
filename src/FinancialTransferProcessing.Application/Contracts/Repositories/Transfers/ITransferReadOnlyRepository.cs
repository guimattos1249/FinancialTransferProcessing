using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Application.Contracts.Repositories.Transfers;

public interface ITransferReadOnlyRepository
{
    Task<Transfer?> GetByIdempotencyKeyAsync(string IdempotencyKey, CancellationToken cancellationToken = default);
}
