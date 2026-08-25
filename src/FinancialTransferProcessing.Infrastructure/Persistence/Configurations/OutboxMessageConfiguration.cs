using FinancialTransferProcessing.Domain.Entities;
using FinancialTransferProcessing.Domain.Validations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FinancialTransferProcessing.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", table =>
        {
            table.HasCheckConstraint(
                "CK_outbox_messages_payload_size",
                $"octet_length(payload) <= {OutboxMessage.MaxPayloadSizeInBytes}");
        });
        builder.HasKey(message => message.MessageId);

        builder.Property(message => message.MessageId).HasColumnName("message_id").ValueGeneratedNever();
        builder.Property(message => message.Type).HasColumnName("type").HasMaxLength(OutboxMessage.MaxTypeLength).IsRequired();
        builder.Property(message => message.SchemaVersion).HasColumnName("schema_version");
        builder.Property(message => message.Payload).HasColumnName("payload").HasColumnType("text").IsRequired();
        builder.Property(message => message.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(message => message.NextAttemptAt).HasColumnName("next_attempt_at");
        builder.Property(message => message.PublishedAt).HasColumnName("published_at");
        builder.Property(message => message.AttemptCount).HasColumnName("attempt_count");
        builder.Property(message => message.LastError).HasColumnName("last_error").HasMaxLength(OutboxMessage.MaxLastErrorLength);
        builder.Property(message => message.CorrelationId).
            HasColumnName("correlation_id").HasMaxLength(DomainValidation.MaxCorrelationIdLength).IsRequired();

        builder.HasIndex(message => new
            {
                message.OccurredAt,
                message.MessageId
            })
            .HasDatabaseName("IX_outbox_messages_publishable_order")
            .HasFilter("published_at IS NULL");

        builder.HasIndex(message => message.NextAttemptAt)
            .HasDatabaseName("IX_outbox_messages_next_attempt")
            .HasFilter("published_at IS NULL");
    }
}
