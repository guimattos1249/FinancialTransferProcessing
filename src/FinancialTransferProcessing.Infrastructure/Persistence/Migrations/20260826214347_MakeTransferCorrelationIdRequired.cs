using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialTransferProcessing.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MakeTransferCorrelationIdRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE transfers
                SET correlation_id = id::text
                WHERE correlation_id IS NULL OR btrim(correlation_id) = '';
                """);

            migrationBuilder.AlterColumn<string>(
                name: "correlation_id",
                table: "transfers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "correlation_id",
                table: "transfers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }
    }
}
