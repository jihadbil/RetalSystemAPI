using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RepairEngineeringAuditV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Stop for operator review rather than rewrite historical codes or quantities.
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1 FROM [Customers]
                    WHERE [Code] IS NOT NULL AND [Code] <> N'' AND [IsDeleted] = 0
                    GROUP BY [TenantId], LTRIM(RTRIM([Code])) HAVING COUNT(*) > 1
                )
                    THROW 51000, N'Customer codes are duplicated. Resolve them before applying RepairEngineeringAuditV2.', 1;
                IF EXISTS (SELECT 1 FROM [StorgeStocks] WHERE [Quantity] < 0 OR [MinStockLevel] < 0)
                    OR EXISTS (SELECT 1 FROM [ShowroomStocks] WHERE [Quantity] < 0 OR [MinStockLevel] < 0)
                    THROW 51001, N'Negative stock exists. Reconcile it before applying RepairEngineeringAuditV2.', 1;
                """);

            migrationBuilder.DropIndex(
                name: "IX_Customers_TenantId_Code",
                table: "Customers");

            migrationBuilder.AddColumn<string>(
                name: "CompanyName",
                table: "Suppliers",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Suppliers",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Suppliers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "Suppliers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "SafeTransactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommercialRegister",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountRate",
                table: "Customers",
                type: "decimal(7,4)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Customers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "Customers",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_StorgeStocks_MinStockLevel",
                table: "StorgeStocks",
                sql: "[MinStockLevel] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_StorgeStocks_Quantity",
                table: "StorgeStocks",
                sql: "[Quantity] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ShowroomStocks_MinStockLevel",
                table: "ShowroomStocks",
                sql: "[MinStockLevel] >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ShowroomStocks_Quantity",
                table: "ShowroomStocks",
                sql: "[Quantity] >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId_Code",
                table: "Customers",
                columns: new[] { "TenantId", "Code" },
                unique: true,
                filter: "[Code] IS NOT NULL AND [Code] <> N'' AND [IsDeleted] = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Customers_DiscountRate",
                table: "Customers",
                sql: "[DiscountRate] >= 0 AND [DiscountRate] <= 100");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_StorgeStocks_MinStockLevel",
                table: "StorgeStocks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_StorgeStocks_Quantity",
                table: "StorgeStocks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ShowroomStocks_MinStockLevel",
                table: "ShowroomStocks");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ShowroomStocks_Quantity",
                table: "ShowroomStocks");

            migrationBuilder.DropIndex(
                name: "IX_Customers_TenantId_Code",
                table: "Customers");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Customers_DiscountRate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "CompanyName",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Suppliers");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "SafeTransactions");

            migrationBuilder.DropColumn(
                name: "CommercialRegister",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "DiscountRate",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Customers");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_TenantId_Code",
                table: "Customers",
                columns: new[] { "TenantId", "Code" });
        }
    }
}
