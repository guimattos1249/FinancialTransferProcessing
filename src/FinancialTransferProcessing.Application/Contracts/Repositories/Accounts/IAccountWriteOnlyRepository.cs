using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Application.Contracts.Repositories.Accounts;

public interface IAccountWriteOnlyRepository
{
    Task Create(Account account);
    Task<bool> Delete(Guid Id);
}
