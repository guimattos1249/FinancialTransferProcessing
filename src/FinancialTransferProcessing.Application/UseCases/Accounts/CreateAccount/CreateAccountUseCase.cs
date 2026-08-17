using FinancialTransferProcessing.Application.Contracts;
using FinancialTransferProcessing.Application.Contracts.Repositories.Accounts;
using FinancialTransferProcessing.Application.Exceptions;
using FinancialTransferProcessing.Domain.Entities;

namespace FinancialTransferProcessing.Application.UseCases.Accounts.CreateAccount;

public class CreateAccountUseCase(IUnitOfWork unitOfWork, IAccountWriteOnlyRepository repository) : ICreateAccountUseCase
{
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IAccountWriteOnlyRepository _repository = repository;

    public async Task<CreateAccountResponse> Execute(CreateAccountRequest request, CancellationToken cancellationToken = default)
    {
        Validate(request);

        var account = new Account(request.Name, request.InitialBalanceInCents);

        await _repository.Create(account);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateAccountResponse(account.Id, account.Name, account.BalanceInCents);
    }

    public static void Validate(CreateAccountRequest request)
    {
        var result = new CreateAccountValidator().Validate(request);

        if (!result.IsValid)
            throw new ErrorOnValidationException(
                [.. result.Errors.Select(e => e.ErrorMessage).Distinct()]);
    }
}
