using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Application.Contracts.Repositories.Accounts;

public interface IAccountReadOnlyRepository
{
    Task<Account> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default);
}
