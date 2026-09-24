using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSalesInvoiceTax : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SalesInvoices_Amounts",
                table: "SalesInvoices");

            migrationBuilder.AddColumn<decimal>(
                name: "TaxAmount",
                table: "SalesInvoices",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SalesInvoices_Amounts",
                table: "SalesInvoices",
                sql: "[SubTotal] >= 0 AND [DiscountAmount] >= 0 AND [TaxAmount] >= 0 AND [TotalAmount] >= 0 AND [PaidAmount] >= 0 AND [RemainingAmount] >= 0 AND [PaidAmount] + [RemainingAmount] = [TotalAmount]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SalesInvoices_Amounts",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "TaxAmount",
                table: "SalesInvoices");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SalesInvoices_Amounts",
                table: "SalesInvoices",
                sql: "[SubTotal] >= 0 AND [DiscountAmount] >= 0 AND [TotalAmount] >= 0 AND [PaidAmount] >= 0 AND [RemainingAmount] >= 0 AND [PaidAmount] + [RemainingAmount] = [TotalAmount]");
        }
    }
}
