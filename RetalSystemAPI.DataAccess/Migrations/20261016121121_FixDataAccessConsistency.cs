using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixDataAccessConsistency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Repair zero balances left active by the old product/barcode soft-delete path.
            // Nonzero balances require explicit reconciliation and must not be hidden here.
            migrationBuilder.Sql("""
                UPDATE s SET [IsDeleted] = 1, [UpdatedAt] = SYSUTCDATETIME()
                FROM [StorgeStocks] s
                INNER JOIN [ProductBarCodes] b ON b.[TenantId] = s.[TenantId] AND b.[Id] = s.[ProductBarcodeId]
                INNER JOIN [Products] p ON p.[TenantId] = b.[TenantId] AND p.[Id] = b.[ProductId]
                WHERE s.[IsDeleted] = 0 AND s.[Quantity] = 0 AND (b.[IsDeleted] = 1 OR p.[IsDeleted] = 1);
                """);

            migrationBuilder.DropIndex(
                name: "IX_Safes_TenantId_BranchId_Name",
                table: "Safes");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseCategories_TenantId_Name",
                table: "ExpenseCategories");

            migrationBuilder.CreateIndex(
                name: "IX_Safes_TenantId_BranchId_Name",
                table: "Safes",
                columns: new[] { "TenantId", "BranchId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_TenantId_Name",
                table: "ExpenseCategories",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Names reused after soft deletion cannot fit the old unfiltered indexes.
            // Refuse before changing the schema rather than deleting or renaming data.
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [Safes] GROUP BY [TenantId], [BranchId], [Name] HAVING COUNT(*) > 1)
                    OR EXISTS (SELECT 1 FROM [ExpenseCategories] GROUP BY [TenantId], [Name] HAVING COUNT(*) > 1)
                    THROW 51000, 'FixDataAccessConsistency: resolve reused safe/category names before downgrading.', 1;
                """);

            // The repair above is intentionally retained: old orphan balances must not be resurrected.
            migrationBuilder.DropIndex(
                name: "IX_Safes_TenantId_BranchId_Name",
                table: "Safes");

            migrationBuilder.DropIndex(
                name: "IX_ExpenseCategories_TenantId_Name",
                table: "ExpenseCategories");

            migrationBuilder.CreateIndex(
                name: "IX_Safes_TenantId_BranchId_Name",
                table: "Safes",
                columns: new[] { "TenantId", "BranchId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseCategories_TenantId_Name",
                table: "ExpenseCategories",
                columns: new[] { "TenantId", "Name" },
                unique: true);
        }
    }
}
