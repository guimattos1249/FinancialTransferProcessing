using FinancialTransferProcessing.Application.Contracts;
using FinancialTransferProcessing.Application.Contracts.Messaging;
using FinancialTransferProcessing.Application.Contracts.Repositories.Accounts;
using FinancialTransferProcessing.Application.Contracts.Repositories.OutboxMessages;
using FinancialTransferProcessing.Application.Contracts.Repositories.Transfers;
using FinancialTransferProcessing.Application.Exceptions;
using FinancialTransferProcessing.Domain.Entities;
using FinancialTransferProcessing.Domain.Exceptions;

namespace FinancialTransferProcessing.Application.UseCases.Transfers.CreateTransfer;

public class CreateTransferUseCase(
    IAccountReadOnlyRepository accountReadOnlyRepository, 
    ITransferReadOnlyRepository transferReadOnlyRepository, 
    ITransferWriteOnlyRepository transferWriteOnlyRepository,
    IOutboxMessageWriteOnlyRepository outboxWriteOnlyRepository,
    IMessageSerializer messageSerializer,
    IUnitOfWork unitOfWork) : ICreateTransferUseCase
{
    private readonly IAccountReadOnlyRepository _accountReadOnlyRepository = accountReadOnlyRepository;
    private readonly ITransferReadOnlyRepository _transferReadOnlyRepository = transferReadOnlyRepository;
    private readonly ITransferWriteOnlyRepository _transferWriteOnlyRepository = transferWriteOnlyRepository;
    private readonly IOutboxMessageWriteOnlyRepository _outboxWriteOnlyRepository = outboxWriteOnlyRepository;
    private readonly IMessageSerializer _messageSerializer = messageSerializer;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    public async Task<CreateTransferResponse> Execute(
        CreateTransferRequest request, 
        Guid idempotencyKey, 
        string? correlationId, 
        CancellationToken cancellationToken = default)
    {
        Validate(request);

        if (idempotencyKey == Guid.Empty)
            throw new ErrorOnValidationException(["Idempotency-Key is required"]);

        if (string.IsNullOrWhiteSpace(correlationId))
            correlationId = Guid.CreateVersion7().ToString();

        var key = idempotencyKey.ToString();

        var existingTransfer = await _transferReadOnlyRepository.GetByIdempotencyKeyAsync(key, cancellationToken);

        if (existingTransfer is not null)
        {
            if (existingTransfer.PayerId != request.PayerId ||
            existingTransfer.PayeeId != request.PayeeId ||
            existingTransfer.AmountInCents != request.AmountInCents)
                throw new ConflictException("A conflict with the idempotency-key has ocurred.");

            return MapToResponse(existingTransfer);
        }

        if (!await _accountReadOnlyRepository.ExistsAsync(request.PayerId, cancellationToken))
            throw new NotFoundException("Payer not found");

        if (!await _accountReadOnlyRepository.ExistsAsync(request.PayeeId, cancellationToken))
            throw new NotFoundException("Payee not found");

        try
        {
            var transfer = new Transfer(request.PayerId, request.PayeeId, request.AmountInCents, key, correlationId);
            
            var messageId = Guid.CreateVersion7();
            var occurredAt = DateTimeOffset.UtcNow;

            var transferRequested = new TransferRequested(messageId, transfer.Id, occurredAt, correlationId, TransferRequested.CurrentSchemaVersion);

            var payload = _messageSerializer.Serialize(transferRequested);
            
            var outboxMessage = new OutboxMessage(
                messageId, 
                TransferRequested.MessageType, 
                TransferRequested.CurrentSchemaVersion,
                payload,
                occurredAt,
                correlationId);


            await _transferWriteOnlyRepository.CreateAsync(transfer, cancellationToken);

            await _outboxWriteOnlyRepository.AddAsync(outboxMessage, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return MapToResponse(transfer);
        }
        catch (DuplicateIdempotencyKeyException)
        {
            existingTransfer =
                await _transferReadOnlyRepository.GetByIdempotencyKeyAsync(
                    idempotencyKey.ToString(),
                    cancellationToken);

            if (existingTransfer is null)
            {
                throw;
            }

            if (existingTransfer.PayerId != request.PayerId ||
                existingTransfer.PayeeId != request.PayeeId ||
                existingTransfer.AmountInCents != request.AmountInCents)
                throw new ConflictException("A conflict with the idempotency-key has ocurred.");

            return MapToResponse(existingTransfer);
        }
        catch (DomainException exception)
        {
            throw new BusinessRuleException(exception.Message);
        }
    }

    public static void Validate(CreateTransferRequest request)
    {
        var result = new CreateTransferValidator().Validate(request);

        if (!result.IsValid)
            throw new ErrorOnValidationException(
                [.. result.Errors.Select(e => e.ErrorMessage).Distinct()]);
    }

    private static CreateTransferResponse MapToResponse(Transfer transfer) => new(transfer.Id, transfer.PayerId, transfer.PayeeId, transfer.AmountInCents, transfer.Status);
}
