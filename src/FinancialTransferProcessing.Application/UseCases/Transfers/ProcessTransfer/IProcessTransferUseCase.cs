namespace FinancialTransferProcessing.Application.UseCases.Transfers.ProcessTransfer;

public interface IProcessTransferUseCase
{
    Task<ProcessTransferResponse> Execute(ProcessTransferRequest request, CancellationToken cancellationToken);
}
