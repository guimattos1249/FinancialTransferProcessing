namespace FinancialTransferProcessing.Application.UseCases.Accounts.CreateAccount;

public record CreateAccountRequest(string Name, long InitialBalanceInCents);
