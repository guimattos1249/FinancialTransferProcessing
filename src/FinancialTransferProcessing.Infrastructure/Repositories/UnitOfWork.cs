using FinancialTransferProcessing.Application.Contracts;
using FinancialTransferProcessing.Application.Exceptions;
using FinancialTransferProcessing.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace FinancialTransferProcessing.Infrastructure.Repositories;

internal sealed class UnitOfWork(ApplicationDbContext dbContext) : IUnitOfWork
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception)
            when (IsIdempotencyKeyViolation(exception))
        {
            throw new DuplicateIdempotencyKeyException();
        }
    }

    private static bool IsIdempotencyKeyViolation(
        DbUpdateException exception)
    {
        return exception.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "IX_transfers_idempotency_key"
        };
    }
}
