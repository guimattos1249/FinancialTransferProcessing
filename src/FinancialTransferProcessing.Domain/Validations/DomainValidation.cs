using FinancialTransferProcessing.Domain.Exceptions;

namespace FinancialTransferProcessing.Domain.Validations;

public static class DomainValidation
{
    public static void EnsurePositiveAmount(long amountInCents)
    {
        if (amountInCents <= 0)
            throw new DomainException("Amount must be greater than zero");
    }
}
