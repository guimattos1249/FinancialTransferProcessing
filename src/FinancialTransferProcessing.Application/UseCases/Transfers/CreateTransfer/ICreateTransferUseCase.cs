namespace FinancialTransferProcessing.Application.UseCases.Transfers.CreateTransfer;

public interface ICreateTransferUseCase
{
    public Task<CreateTransferResponse> Execute(CreateTransferRequest request, Guid idempotencyKey, CancellationToken cancellationToken = default);
}
