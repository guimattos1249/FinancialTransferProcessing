using FluentValidation;

namespace FinancialTransferProcessing.Application.UseCases.Transfers.CreateTransfer;

public class CreateTransferValidator : AbstractValidator<CreateTransferRequest>
{
    public CreateTransferValidator()
    {
        RuleFor(t => t.PayerId)
            .NotEmpty().WithMessage("PayerId is required.");
        RuleFor(t => t.PayeeId)
            .NotEmpty().WithMessage("PayeeId is required.");
        RuleFor(t => t.AmountInCents)
            .GreaterThan(0).WithMessage("The transfer amount must be greater than zero.");
    }
}
