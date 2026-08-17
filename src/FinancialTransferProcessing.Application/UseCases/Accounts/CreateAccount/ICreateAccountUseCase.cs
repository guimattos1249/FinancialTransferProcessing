namespace FinancialTransferProcessing.Application.UseCases.Accounts.CreateAccount;

public interface ICreateAccountUseCase
{
    public Task<CreateAccountResponse> Execute(CreateAccountRequest request, CancellationToken cancellationToken = default);
}
