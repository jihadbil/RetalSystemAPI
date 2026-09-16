using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddReasonToStockAdjustmentItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1 = جرد دوري (InventoryCount) — بداية الـ Enum، فالقيمة 0 غير صالحة
            migrationBuilder.AddColumn<int>(
                name: "Reason",
                table: "StockAdjustmentItems",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // تعبئة البنود التاريخية من سبب التسوية العام بدل تركها على الافتراضي
            migrationBuilder.Sql(@"
UPDATE sai
SET sai.Reason = sa.Reason
FROM StockAdjustmentItems sai
INNER JOIN StockAdjustments sa ON sa.Id = sai.StockAdjustmentId
WHERE sai.Reason = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "StockAdjustmentItems");
        }
    }
}
