using FinancialTransferProcessing.Application.Contracts;
using FinancialTransferProcessing.Infrastructure.Persistence;

namespace FinancialTransferProcessing.Infrastructure.Repositories;

internal sealed class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
