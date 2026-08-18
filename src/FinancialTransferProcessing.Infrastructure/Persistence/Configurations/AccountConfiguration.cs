using FinancialTransferProcessing.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTransferProcessing.Infrastructure.Persistence.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts");
        builder.HasKey(account => account.Id);

        builder.Property(account => account.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(account => account.Name).HasColumnName("name").HasMaxLength(Account.MaxNameLength).IsRequired();
        builder.Property(account => account.BalanceInCents).HasColumnName("balance_in_cents").IsRequired();
        builder.Property(account => account.Version).HasColumnName("version").IsConcurrencyToken().IsRequired();
        builder.Property(account => account.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(account => account.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(account => account.DeletedAt).HasColumnName("deleted_at");

        builder.HasQueryFilter(account => account.DeletedAt == null);
    }
}
