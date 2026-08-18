namespace FinancialTransferProcessing.Application.Contracts;

public interface IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
