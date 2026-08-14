using FinancialTransferProcessing.Domain.Exceptions;
using FinancialTransferProcessing.Domain.Validations;

namespace FinancialTransferProcessing.Domain.Entities;

public class Account : EntityBase
{
    public const int MaxNameLength = 100;
    public const int MinNameLength = 3;

    public Account(string name, long initialBalanceInCents = 0)
    {
        Name = ValidateName(name);

        if (initialBalanceInCents < 0)
            throw new DomainException("Initial balance cannot be negative.");

        BalanceInCents = initialBalanceInCents;
    }

    public string Name { get; private set; }
    public long BalanceInCents { get; private set; }
    public long Version { get; private set; }
    private readonly List<Transfer> _outgoingTransfers = [];
    private readonly List<Transfer> _incomingTransfers = [];

    public IReadOnlyCollection<Transfer> OutgoingTransfers =>
        _outgoingTransfers.AsReadOnly();

    public IReadOnlyCollection<Transfer> IncomingTransfers =>
        _incomingTransfers.AsReadOnly();

    public void SetName(string name)
    {
        Name = ValidateName(name);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Account name cannot be empty.");

        name = name.Trim();

        if (name.Length is < MinNameLength or > MaxNameLength)
            throw new DomainException(
                $"Account name must have between {MinNameLength} and {MaxNameLength} characters.");

        return name;
    }

    public void Debit(long amountInCents)
    {
        DomainValidation.EnsurePositiveAmount(amountInCents);

        if (BalanceInCents < amountInCents)
            throw new DomainException("Insufficient Balance");

        BalanceInCents -= amountInCents;
        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Credit(long amountInCents)
    {
        DomainValidation.EnsurePositiveAmount(amountInCents);

        try
        {
            BalanceInCents = checked(BalanceInCents + amountInCents);
        }
        catch (OverflowException exception)
        {
            throw new DomainException(
                "The credit would exceed the maximum supported balance.",
                exception);
        }

        Version++;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}
