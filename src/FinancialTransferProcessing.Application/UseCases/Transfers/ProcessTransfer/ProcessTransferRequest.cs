namespace FinancialTransferProcessing.Application.UseCases.Transfers.ProcessTransfer;

public record ProcessTransferRequest(Guid MessageId, Guid TransferId, DateTimeOffset OccurredAt, Guid CorrelationId);
