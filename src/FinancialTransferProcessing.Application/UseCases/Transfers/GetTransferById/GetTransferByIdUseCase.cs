using FinancialTransferProcessing.Application.Contracts.Repositories.Transfers;
using FinancialTransferProcessing.Application.Exceptions;

namespace FinancialTransferProcessing.Application.UseCases.Transfers.GetTransferById;

public class GetTransferByIdUseCase(ITransferReadOnlyRepository transferRepository) : IGetTransferByIdUseCase
{
    private readonly ITransferReadOnlyRepository _transferRepository = transferRepository;

    public async Task<GetTransferByIdResponse> Execute(Guid Id, CancellationToken cancellationToken)
    {
        if (Id == Guid.Empty)
            throw new ErrorOnValidationException(["Transfer Id is required."]);

        var transfer = await _transferRepository.GetByIdAsync(Id, cancellationToken) ?? throw new NotFoundException("Transfer not found.");

        return new GetTransferByIdResponse(
            transfer.Id,
            transfer.PayerId,
            transfer.PayeeId,
            transfer.AmountInCents,
            transfer.Status,
            transfer.CreatedAt,
            transfer.ProcessedAt,
            transfer.FailureReason);
    }
}
