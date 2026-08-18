namespace FinancialTransferProcessing.Application.UseCases.Accounts.GetAccountById;

public interface IGetAccountByIdUseCase
{
    Task<GetAccountByIdResponse> Execute(Guid Id, CancellationToken cancellationToken);
}
