using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialTransferProcessing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOutboxPublisherLease : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "lease_expires_at",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "lease_id",
                table: "outbox_messages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_lease_expires",
                table: "outbox_messages",
                column: "lease_expires_at",
                filter: "published_at IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_lease_expires",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "lease_expires_at",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "lease_id",
                table: "outbox_messages");
        }
    }
}
