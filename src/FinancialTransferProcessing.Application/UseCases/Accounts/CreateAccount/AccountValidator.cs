using FinancialTransferProcessing.Domain.Entities;
using FluentValidation;

namespace FinancialTransferProcessing.Application.UseCases.Accounts.CreateAccount;

public class CreateAccountValidator : AbstractValidator<CreateAccountRequest>
{
    public CreateAccountValidator()
    {
        RuleFor(a => a.Name)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Name cannot be empty")
            .Must(name => name.Trim().Length >= Account.MinNameLength)
                .WithMessage($"Name must have at least {Account.MinNameLength} characters")
            .Must(name => name.Trim().Length <= Account.MaxNameLength)
                .WithMessage($"Name must have at most {Account.MaxNameLength} characters");
        RuleFor(a => a.InitialBalanceInCents)
            .GreaterThanOrEqualTo(0).WithMessage("The initial balance must be greater or equal to 0.");
    }
}
