using FinancialTransferProcessing.Domain.Enums;

namespace FinancialTransferProcessing.Application.UseCases.Transfers.GetTransferById;

public record GetTransferByIdResponse(
    Guid Id, 
    Guid PayerId, 
    Guid PayeeId, 
    long AmountInCents, 
    ETransferStatus Status, 
    DateTimeOffset CreatedAt,
    DateTimeOffset? ProcessedAt,
    string? FailureReason);
