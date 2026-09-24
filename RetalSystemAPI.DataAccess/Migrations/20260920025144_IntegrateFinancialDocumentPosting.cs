using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class IntegrateFinancialDocumentPosting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PaymentMethod",
                table: "SalesReturns",
                type: "int",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.AddColumn<Guid>(
                name: "RefundSafeId",
                table: "SalesReturns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RefundedAmount",
                table: "SalesReturns",
                type: "decimal(18,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentSafeId",
                table: "SalesInvoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RefundSafeId",
                table: "PurchaseReturns",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaymentSafeId",
                table: "PurchaseInvoices",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_RefundSafeId",
                table: "SalesReturns",
                column: "RefundSafeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_PaymentSafeId",
                table: "SalesInvoices",
                column: "PaymentSafeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_RefundSafeId",
                table: "PurchaseReturns",
                column: "RefundSafeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_PaymentSafeId",
                table: "PurchaseInvoices",
                column: "PaymentSafeId");

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Safes_PaymentSafeId",
                table: "PurchaseInvoices",
                column: "PaymentSafeId",
                principalTable: "Safes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Safes_RefundSafeId",
                table: "PurchaseReturns",
                column: "RefundSafeId",
                principalTable: "Safes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_Safes_PaymentSafeId",
                table: "SalesInvoices",
                column: "PaymentSafeId",
                principalTable: "Safes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Safes_RefundSafeId",
                table: "SalesReturns",
                column: "RefundSafeId",
                principalTable: "Safes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Safes_PaymentSafeId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Safes_RefundSafeId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Safes_PaymentSafeId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Safes_RefundSafeId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_RefundSafeId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_PaymentSafeId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_RefundSafeId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_PaymentSafeId",
                table: "PurchaseInvoices");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "RefundSafeId",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "RefundedAmount",
                table: "SalesReturns");

            migrationBuilder.DropColumn(
                name: "PaymentSafeId",
                table: "SalesInvoices");

            migrationBuilder.DropColumn(
                name: "RefundSafeId",
                table: "PurchaseReturns");

            migrationBuilder.DropColumn(
                name: "PaymentSafeId",
                table: "PurchaseInvoices");
        }
    }
}
