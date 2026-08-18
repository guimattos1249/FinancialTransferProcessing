using FinancialTransferProcessing.Application.Contracts.Repositories.Accounts;
using FinancialTransferProcessing.Application.Exceptions;

namespace FinancialTransferProcessing.Application.UseCases.Accounts.GetAccountById;

public class GetAccountByIdUseCase(IAccountReadOnlyRepository repository) : IGetAccountByIdUseCase
{
    private readonly IAccountReadOnlyRepository _repository = repository;

    public async Task<GetAccountByIdResponse> Execute(Guid Id, CancellationToken cancellationToken)
    {
        var account = await _repository.GetByIdAsync(Id, cancellationToken) ?? throw new NotFoundException("Account not found");

        return new GetAccountByIdResponse(
            account.Id, 
            account.Name, 
            account.BalanceInCents, 
            account.Version, 
            account.CreatedAt, 
            account.UpdatedAt);
    }
}
