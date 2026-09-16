using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FilterBarcodeUniqueIndexForSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductBarCodes_TenantId_BarCode",
                table: "ProductBarCodes");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarCodes_TenantId_BarCode",
                table: "ProductBarCodes",
                columns: new[] { "TenantId", "BarCode" },
                unique: true,
                filter: "[IsDeleted] = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProductBarCodes_TenantId_BarCode",
                table: "ProductBarCodes");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarCodes_TenantId_BarCode",
                table: "ProductBarCodes",
                columns: new[] { "TenantId", "BarCode" },
                unique: true);
        }
    }
}
