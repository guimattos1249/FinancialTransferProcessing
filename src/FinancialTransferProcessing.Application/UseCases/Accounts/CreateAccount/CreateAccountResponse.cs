namespace FinancialTransferProcessing.Application.UseCases.Accounts.CreateAccount;

public record CreateAccountResponse(Guid Id, string Name, long BalanceInCents);
