using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Application.Contracts.Repositories.Accounts;

public interface IAccountWriteOnlyRepository
{
    Task CreateAsync(Account account, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid Id);
}
