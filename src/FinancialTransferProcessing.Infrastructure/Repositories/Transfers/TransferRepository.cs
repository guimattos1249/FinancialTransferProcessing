using FinancialTransferProcessing.Application.Contracts.Repositories.Transfers;
using FinancialTransferProcessing.Domain.Entities;
using FinancialTransferProcessing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinancialTransferProcessing.Infrastructure.Repositories.Transfers;

public sealed class TransferRepository(ApplicationDbContext context)
    : ITransferReadOnlyRepository, ITransferWriteOnlyRepository
{
    public async Task CreateAsync(
        Transfer transfer,
        CancellationToken cancellationToken = default) =>
        await context.Transfers.AddAsync(transfer, cancellationToken);

    public async Task<Transfer?> GetByIdAsync(Guid Id, CancellationToken cancellationToken = default) =>
        await context.Transfers.AsNoTracking().SingleOrDefaultAsync(t => t.Id == Id, cancellationToken);

    public Task<Transfer?> GetByIdempotencyKeyAsync(
        string idempotencyKey,
        CancellationToken cancellationToken = default) => context.Transfers
            .AsNoTracking()
            .SingleOrDefaultAsync(
                transfer => transfer.IdempotencyKey == idempotencyKey,
                cancellationToken);
}
