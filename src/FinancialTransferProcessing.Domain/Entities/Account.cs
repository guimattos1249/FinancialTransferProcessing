namespace FinancialTransferProcessing.Domain.Entities;

public class Account : EntityBase
{
    public string Name { get; private set; }
    public string BalanceInCents { get; private set; }
    public long Version { get; private set; }
    public ICollection<Transfer?> OutgoingTransfers { get; set; } = [];
    public ICollection<Transfer?> IncomingTransfers { get; set; } = [];
}
