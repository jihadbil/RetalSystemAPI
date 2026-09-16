using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseInvoiceItemBreakdownsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PurchaseInvoiceItemBreakdowns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PurchaseInvoiceItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProductBarCodeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PackageQuantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitsPerPackage = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseInvoiceItemBreakdowns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceItemBreakdowns_ProductBarCodes_ProductBarCodeId",
                        column: x => x.ProductBarCodeId,
                        principalTable: "ProductBarCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceItemBreakdowns_PurchaseInvoiceItems_PurchaseInvoiceItemId",
                        column: x => x.PurchaseInvoiceItemId,
                        principalTable: "PurchaseInvoiceItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseInvoiceItemBreakdowns_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceItemBreakdowns_ProductBarCodeId",
                table: "PurchaseInvoiceItemBreakdowns",
                column: "ProductBarCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceItemBreakdowns_PurchaseInvoiceItemId",
                table: "PurchaseInvoiceItemBreakdowns",
                column: "PurchaseInvoiceItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceItemBreakdowns_TenantId_PurchaseInvoiceItemId_ProductBarCodeId",
                table: "PurchaseInvoiceItemBreakdowns",
                columns: new[] { "TenantId", "PurchaseInvoiceItemId", "ProductBarCodeId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseInvoiceItemBreakdowns");
        }
    }
}
