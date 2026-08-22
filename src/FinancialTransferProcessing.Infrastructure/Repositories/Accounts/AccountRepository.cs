using FinancialTransferProcessing.Application.Contracts.Repositories.Accounts;
using FinancialTransferProcessing.Domain.Entities;
using FinancialTransferProcessing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransferProcessing.Infrastructure.Repositories.Accounts;

public sealed class AccountRepository(ApplicationDbContext context)
    : IAccountReadOnlyRepository, IAccountWriteOnlyRepository
{
    public async Task CreateAsync(Account account, CancellationToken cancellationToken = default)
    {
        await context.Accounts.AddAsync(account, cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid Id)
    {
        var account = await context.Accounts.FindAsync(Id);

        if (account is null)
            return false;

        context.Accounts.Remove(account);
        return true;
    }

    public Task<bool> ExistsAsync(Guid Id, CancellationToken cancellationToken = default) => context.Accounts
        .AsNoTracking()
        .AnyAsync(x => x.Id == Id, cancellationToken);

    public Task<Account?> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default) => context.Accounts
            .AsNoTracking()
            .SingleOrDefaultAsync(account => account.Id == Id, cancellationToken);
}
