namespace FinancialTransferProcessing.Application.UseCases.Accounts.GetAccountById;

public record GetAccountByIdResponse(
    Guid Id, 
    string Name, 
    long BalanceInCents, 
    long Version, 
    DateTimeOffset CreatedAt, 
    DateTimeOffset UpdatedAt);
