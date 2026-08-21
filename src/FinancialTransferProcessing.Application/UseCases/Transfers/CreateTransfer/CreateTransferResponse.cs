using FinancialTransferProcessing.Domain.Enums;

namespace FinancialTransferProcessing.Application.UseCases.Transfers.CreateTransfer;

public record CreateTransferResponse(Guid Id, Guid PayerId, Guid PayeeId, long AmountInCents, ETransferStatus Status);
