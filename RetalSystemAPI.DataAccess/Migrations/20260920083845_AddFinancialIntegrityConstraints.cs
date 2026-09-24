using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFinancialIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE [SalesInvoices]
                SET [SubTotal] = CASE WHEN [SubTotal] < 0 THEN 0 ELSE [SubTotal] END,
                    [DiscountAmount] = CASE WHEN [DiscountAmount] < 0 THEN 0 ELSE [DiscountAmount] END,
                    [TotalAmount] = CASE WHEN [TotalAmount] < 0 THEN 0 ELSE [TotalAmount] END;
                UPDATE [SalesInvoices]
                SET [PaidAmount] = CASE WHEN [PaidAmount] < 0 THEN 0 WHEN [PaidAmount] > [TotalAmount] THEN [TotalAmount] ELSE [PaidAmount] END;
                UPDATE [SalesInvoices] SET [RemainingAmount] = [TotalAmount] - [PaidAmount];

                UPDATE [PurchaseInvoices]
                SET [SubTotal] = CASE WHEN [SubTotal] < 0 THEN 0 ELSE [SubTotal] END,
                    [DiscountAmount] = CASE WHEN [DiscountAmount] < 0 THEN 0 ELSE [DiscountAmount] END,
                    [TaxAmount] = CASE WHEN [TaxAmount] < 0 THEN 0 ELSE [TaxAmount] END,
                    [TotalAmount] = CASE WHEN [TotalAmount] < 0 THEN 0 ELSE [TotalAmount] END;
                UPDATE [PurchaseInvoices]
                SET [PaidAmount] = CASE WHEN [PaidAmount] < 0 THEN 0 WHEN [PaidAmount] > [TotalAmount] THEN [TotalAmount] ELSE [PaidAmount] END;
                UPDATE [PurchaseInvoices] SET [RemainingAmount] = [TotalAmount] - [PaidAmount];

                UPDATE [SalesReturns]
                SET [TotalAmount] = CASE WHEN [TotalAmount] < 0 THEN 0 ELSE [TotalAmount] END;
                UPDATE [SalesReturns]
                SET [RefundedAmount] = CASE WHEN [RefundedAmount] < 0 THEN 0 WHEN [RefundedAmount] > [TotalAmount] THEN [TotalAmount] ELSE [RefundedAmount] END;
                UPDATE [PurchaseReturns]
                SET [TotalAmount] = CASE WHEN [TotalAmount] < 0 THEN 0 ELSE [TotalAmount] END;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_SalesReturns_Amounts",
                table: "SalesReturns",
                sql: "[TotalAmount] >= 0 AND [RefundedAmount] >= 0 AND [RefundedAmount] <= [TotalAmount]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SalesInvoices_Amounts",
                table: "SalesInvoices",
                sql: "[SubTotal] >= 0 AND [DiscountAmount] >= 0 AND [TotalAmount] >= 0 AND [PaidAmount] >= 0 AND [RemainingAmount] >= 0 AND [PaidAmount] + [RemainingAmount] = [TotalAmount]");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseReturns_TotalAmount",
                table: "PurchaseReturns",
                sql: "[TotalAmount] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseInvoices_Amounts",
                table: "PurchaseInvoices",
                sql: "[SubTotal] >= 0 AND [DiscountAmount] >= 0 AND [TaxAmount] >= 0 AND [TotalAmount] >= 0 AND [PaidAmount] >= 0 AND [RemainingAmount] >= 0 AND [PaidAmount] + [RemainingAmount] = [TotalAmount]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_SalesReturns_Amounts",
                table: "SalesReturns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SalesInvoices_Amounts",
                table: "SalesInvoices");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseReturns_TotalAmount",
                table: "PurchaseReturns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseInvoices_Amounts",
                table: "PurchaseInvoices");
        }
    }
}
