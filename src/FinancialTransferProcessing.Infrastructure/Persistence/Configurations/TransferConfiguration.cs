using FinancialTransferProcessing.Domain.Entities;
using FinancialTransferProcessing.Domain.Validations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTransferProcessing.Infrastructure.Persistence.Configurations;

internal sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("transfers");
        builder.HasKey(transfer => transfer.Id);

        builder.Property(transfer => transfer.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(transfer => transfer.PayerId).HasColumnName("payer_id").IsRequired();
        builder.Property(transfer => transfer.PayeeId).HasColumnName("payee_id").IsRequired();
        builder.Property(transfer => transfer.AmountInCents).HasColumnName("amount_in_cents").IsRequired();
        builder.Property(transfer => transfer.IdempotencyKey).HasColumnName("idempotency_key").HasMaxLength(Transfer.MaxIdempotencyKeyLength).IsRequired();
        builder.Property(transfer => transfer.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(transfer => transfer.ProcessedAt).HasColumnName("processed_at");
        builder.Property(transfer => transfer.FailureReason).HasColumnName("failure_reason");
        builder.Property(message => message.CorrelationId).
            HasColumnName("correlation_id").HasMaxLength(DomainValidation.MaxCorrelationIdLength).IsRequired();
        builder.Property(transfer => transfer.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(transfer => transfer.UpdatedAt).HasColumnName("updated_at").IsRequired();
        builder.Property(transfer => transfer.DeletedAt).HasColumnName("deleted_at");

        builder.HasIndex(transfer => transfer.IdempotencyKey).IsUnique();
        builder.HasQueryFilter(transfer => transfer.DeletedAt == null);

        builder.HasOne(transfer => transfer.Payer)
            .WithMany(account => account.OutgoingTransfers)
            .HasForeignKey(transfer => transfer.PayerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(transfer => transfer.Payee)
            .WithMany(account => account.IncomingTransfers)
            .HasForeignKey(transfer => transfer.PayeeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
