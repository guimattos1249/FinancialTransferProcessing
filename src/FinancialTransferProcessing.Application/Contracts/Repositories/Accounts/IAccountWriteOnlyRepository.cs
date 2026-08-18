using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Application.Contracts.Repositories.Accounts;

public interface IAccountWriteOnlyRepository
{
    Task Create(Account account, CancellationToken cancellationToken = default);
    Task<bool> Delete(Guid Id);
}
