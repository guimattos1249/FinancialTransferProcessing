namespace FinancialTransferProcessing.Application.UseCases.Transfers.CreateTransfer;

public interface ICreateTransferUseCase
{
    public Task<CreateTransferResponse> Execute(
        CreateTransferRequest request, 
        Guid idempotencyKey, 
        string correlationId, 
        CancellationToken cancellationToken = default);
}
