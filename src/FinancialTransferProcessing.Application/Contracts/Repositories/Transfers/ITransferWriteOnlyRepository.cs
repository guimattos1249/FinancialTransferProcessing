using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Application.Contracts.Repositories.Transfers;

public interface ITransferWriteOnlyRepository
{
    Task CreateAsync(Transfer transfer, CancellationToken cancellationToken = default);
}
