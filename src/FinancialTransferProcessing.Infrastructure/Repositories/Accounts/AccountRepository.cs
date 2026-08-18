using FinancialTransferProcessing.Application.Contracts.Repositories.Accounts;
using FinancialTransferProcessing.Domain.Entities;
using FinancialTransferProcessing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransferProcessing.Infrastructure.Repositories.Accounts;

internal sealed class AccountRepository(ApplicationDbContext context)
    : IAccountReadOnlyRepository, IAccountWriteOnlyRepository
{
    public async Task Create(Account account)
    {
        await context.Accounts.AddAsync(account);
    }

    public async Task<bool> Delete(Guid Id)
    {
        var account = await context.Accounts.FindAsync(Id);

        if (account is null)
            return false;

        context.Accounts.Remove(account);
        return true;
    }

    public Task<Account> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default)
    {
        return context.Accounts
            .AsNoTracking()
            .SingleAsync(account => account.Id == Id, cancellationToken);
    }
}
