namespace FinancialTransferProcessing.Application.UseCases.Transfers.CreateTransfer;

public record CreateTransferRequest(Guid PayerId, Guid PayeeId, long AmountInCents);
