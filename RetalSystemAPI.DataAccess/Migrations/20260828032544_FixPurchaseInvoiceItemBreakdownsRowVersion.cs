using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixPurchaseInvoiceItemBreakdownsRowVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItemBreakdowns_Tenants_TenantId",
                table: "PurchaseInvoiceItemBreakdowns");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PurchaseInvoiceItemBreakdowns");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PurchaseInvoiceItemBreakdowns",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItemBreakdowns_Tenants_TenantId",
                table: "PurchaseInvoiceItemBreakdowns",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItemBreakdowns_Tenants_TenantId",
                table: "PurchaseInvoiceItemBreakdowns");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PurchaseInvoiceItemBreakdowns");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PurchaseInvoiceItemBreakdowns",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItemBreakdowns_Tenants_TenantId",
                table: "PurchaseInvoiceItemBreakdowns",
                column: "TenantId",
                principalTable: "Tenants",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
