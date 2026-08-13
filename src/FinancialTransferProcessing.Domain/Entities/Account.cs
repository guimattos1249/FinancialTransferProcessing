using FinancialTransferProcessing.Domain.Exceptions;
using System.Text.RegularExpressions;

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
    public ICollection<Transfer> OutgoingTransfers { get; private set; } = [];
    public ICollection<Transfer> IncomingTransfers { get; private set; } = [];

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
        EnsurePositiveAmount(amountInCents);

        if (BalanceInCents < amountInCents)
            throw new DomainException("Insufficient Balance");

        BalanceInCents -= amountInCents;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    public void Credit(long amountInCents)
    {
        EnsurePositiveAmount(amountInCents);

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

        UpdatedAt = DateTimeOffset.UtcNow;
    }

    private static void EnsurePositiveAmount(long amountInCents)
    {
        if (amountInCents <= 0)
            throw new DomainException("Amount must be greater than zero");
    }
}
