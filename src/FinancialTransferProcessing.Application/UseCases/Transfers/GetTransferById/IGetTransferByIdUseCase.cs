namespace FinancialTransferProcessing.Application.UseCases.Transfers.GetTransferById;

public interface IGetTransferByIdUseCase
{
    Task<GetTransferByIdResponse> Execute(Guid Id, CancellationToken cancellationToken);
}
