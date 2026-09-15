using FinancialTransferProcessing.Domain.Enums;

namespace FinancialTransferProcessing.Application.UseCases.Transfers.ProcessTransfer;

public record ProcessTransferResponse(Guid TransferId, ETransferStatus Status);
