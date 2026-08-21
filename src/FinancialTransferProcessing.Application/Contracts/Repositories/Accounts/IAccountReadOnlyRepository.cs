using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Application.Contracts.Repositories.Accounts;

public interface IAccountReadOnlyRepository
{
    Task<Account?> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(Guid Id, CancellationToken cancellationToken = default);
}
