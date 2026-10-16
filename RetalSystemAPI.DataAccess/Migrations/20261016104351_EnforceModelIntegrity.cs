using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RetalSystemAPI.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class EnforceModelIntegrity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Abort before changing the schema; legacy quantities must never be rounded or truncated.
            foreach (var table in new[] { "PurchaseInvoiceItems", "PurchaseInvoiceItemBreakdowns", "PurchaseOrderItems", "PurchaseReturnItems" })
                migrationBuilder.Sql($"""
                    IF EXISTS (SELECT 1 FROM [{table}] WHERE [Quantity] <= 0 OR [Quantity] > 2147483647 OR [Quantity] <> FLOOR([Quantity]))
                        THROW 51000, 'EnforceModelIntegrity: {table} contains invalid smallest-unit quantities. Correct these records before retrying.', 1;
                    """);

            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM [PurchaseInvoiceItemBreakdowns] WHERE [PackageQuantity] <= 0 OR [UnitsPerPackage] <= 0 OR [Quantity] <> [PackageQuantity] * [UnitsPerPackage])
                    THROW 51000, 'EnforceModelIntegrity: package quantities do not match smallest-unit counts.', 1;
                IF EXISTS (SELECT 1 FROM [ProductUnits] WHERE [ConversionFactor] <= 0)
                    OR EXISTS (SELECT 1 FROM [Units] WHERE [UnitPackage] <= 0)
                    THROW 51000, 'EnforceModelIntegrity: invalid unit conversion factors.', 1;
                IF EXISTS (SELECT 1 FROM [ProductUnits] WHERE [IsDefault] = 1 AND [IsDeleted] = 0 GROUP BY [TenantId], [ProductId] HAVING COUNT(*) > 1)
                    OR EXISTS (SELECT 1 FROM [ProductImages] WHERE [IsDefault] = 1 AND [IsDeleted] = 0 GROUP BY [TenantId], [ProductId] HAVING COUNT(*) > 1)
                    THROW 51000, 'EnforceModelIntegrity: choose a single default unit/image per product before retrying.', 1;
                IF EXISTS (SELECT 1 FROM [PurchaseInvoices] WHERE ABS([SubTotal]) >= 100000000000000 OR ABS([DiscountAmount]) >= 100000000000000 OR ABS([TaxAmount]) >= 100000000000000 OR ABS([TotalAmount]) >= 100000000000000 OR ABS([PaidAmount]) >= 100000000000000 OR ABS([RemainingAmount]) >= 100000000000000)
                    OR EXISTS (SELECT 1 FROM [PurchaseInvoiceItems] WHERE ABS([UnitPrice]) >= 100000000000000 OR ABS([LineTotal]) >= 100000000000000 OR ABS([DiscountAmount]) >= 100000000000000)
                    THROW 51000, 'EnforceModelIntegrity: monetary values exceed decimal(18,4) capacity.', 1;
                """);

            // Preserve the existing financial constraint while changing its column types.
            migrationBuilder.DropCheckConstraint("CK_PurchaseInvoices_Amounts", "PurchaseInvoices");
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Branches_BranchId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchPhones_Branches_BranchId",
                table: "BranchPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_CashSessions_Branches_BranchId",
                table: "CashSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_CashSessions_PosTerminals_PosTerminalId",
                table: "CashSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_CashSessions_Safes_CashDrawerId",
                table: "CashSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPhones_Customers_CustomerId",
                table: "CustomerPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerTransactions_Branches_BranchId",
                table: "CustomerTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerTransactions_Customers_CustomerId",
                table: "CustomerTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Branches_BranchId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_ExpenseCategories_ExpenseCategoryId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Safes_SafeId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_PosDeviceCredentials_PosTerminals_PosTerminalId",
                table: "PosDeviceCredentials");

            migrationBuilder.DropForeignKey(
                name: "FK_PosTerminals_Branches_BranchId",
                table: "PosTerminals");

            migrationBuilder.DropForeignKey(
                name: "FK_PosTerminals_Safes_DefaultCashDrawerId",
                table: "PosTerminals");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductBarCodes_Products_ProductId",
                table: "ProductBarCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductBarCodes_BarcodeId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnits_Products_ProductId",
                table: "ProductUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnits_Units_UnitId",
                table: "ProductUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItemBreakdowns_ProductBarCodes_ProductBarCodeId",
                table: "PurchaseInvoiceItemBreakdowns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItemBreakdowns_PurchaseInvoiceItems_PurchaseInvoiceItemId",
                table: "PurchaseInvoiceItemBreakdowns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItems_ProductBarCodes_ProductBarCodeId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItems_Products_ProductId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItems_PurchaseInvoices_PurchaseInvoiceId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Branches_BranchId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Safes_PaymentSafeId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Suppliers_SupplierId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Warehouses_WarehouseId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_ProductBarCodes_ProductBarCodeId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Branches_BranchId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Warehouses_WarehouseId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturnItems_ProductBarCodes_ProductBarCodeId",
                table: "PurchaseReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturnItems_Products_ProductId",
                table: "PurchaseReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturnItems_PurchaseReturns_PurchaseReturnId",
                table: "PurchaseReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Branches_BranchId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_PurchaseInvoices_PurchaseInvoiceId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Safes_RefundSafeId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Suppliers_SupplierId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Warehouses_WarehouseId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_Safes_Branches_BranchId",
                table: "Safes");

            migrationBuilder.DropForeignKey(
                name: "FK_SafeTransactions_CashSessions_CashSessionId",
                table: "SafeTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SafeTransactions_Safes_SafeId",
                table: "SafeTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceItems_ProductBarCodes_ProductBarCodeId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceItems_Products_ProductId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceItems_SalesInvoices_SalesInvoiceId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Branches_BranchId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_CashSessions_CashSessionId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Customers_CustomerId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_PosTerminals_PosTerminalId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Safes_PaymentSafeId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Warehouses_WarehouseId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturnItems_ProductBarCodes_ProductBarCodeId",
                table: "SalesReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturnItems_Products_ProductId",
                table: "SalesReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturnItems_SalesReturns_SalesReturnId",
                table: "SalesReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Branches_BranchId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Customers_CustomerId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Safes_RefundSafeId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_SalesInvoices_OriginalInvoiceId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Warehouses_WarehouseId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_ShowroomStocks_Products_ProductId",
                table: "ShowroomStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_ShowroomStocks_Warehouses_WarehouseId",
                table: "ShowroomStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAdjustmentItems_ProductBarCodes_ProductBarCodeId",
                table: "StockAdjustmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAdjustmentItems_Products_ProductId",
                table: "StockAdjustmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAdjustmentItems_StockAdjustments_StockAdjustmentId",
                table: "StockAdjustmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAdjustments_Warehouses_WarehouseId",
                table: "StockAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransferItems_ProductBarCodes_ProductBarCodeId",
                table: "StockTransferItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransferItems_Products_ProductId",
                table: "StockTransferItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransferItems_StockTransfers_StockTransferId",
                table: "StockTransferItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransfers_Warehouses_FromWarehouseId",
                table: "StockTransfers");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransfers_Warehouses_ToWarehouseId",
                table: "StockTransfers");

            migrationBuilder.DropForeignKey(
                name: "FK_StorgeStocks_ProductBarCodes_ProductBarcodeId",
                table: "StorgeStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_StorgeStocks_Warehouses_WarehouseId",
                table: "StorgeStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierPhones_Suppliers_SupplierId",
                table: "SupplierPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierTransactions_Branches_BranchId",
                table: "SupplierTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierTransactions_Suppliers_SupplierId",
                table: "SupplierTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Branches_BranchId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Warehouses_BranchId",
                table: "Warehouses");

            migrationBuilder.DropIndex(
                name: "IX_Units_TenantId_Name",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Name",
                table: "Tenants");

            migrationBuilder.DropIndex(
                name: "IX_SupplierTransactions_BranchId",
                table: "SupplierTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SupplierTransactions_SupplierId",
                table: "SupplierTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SupplierPhones_TenantId_PhoneNumber",
                table: "SupplierPhones");

            migrationBuilder.DropIndex(
                name: "IX_StorgeStocks_ProductBarcodeId",
                table: "StorgeStocks");

            migrationBuilder.DropIndex(
                name: "IX_StorgeStocks_WarehouseId_ProductBarcodeId",
                table: "StorgeStocks");

            migrationBuilder.DropIndex(
                name: "IX_StockTransfers_FromWarehouseId",
                table: "StockTransfers");

            migrationBuilder.DropIndex(
                name: "IX_StockTransfers_ToWarehouseId",
                table: "StockTransfers");

            migrationBuilder.DropIndex(
                name: "IX_StockAdjustments_WarehouseId",
                table: "StockAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_ShowroomStocks_ProductId",
                table: "ShowroomStocks");

            migrationBuilder.DropIndex(
                name: "IX_ShowroomStocks_WarehouseId_ProductId",
                table: "ShowroomStocks");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_BranchId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_CustomerId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_OriginalInvoiceId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_RefundSafeId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_WarehouseId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_BranchId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_CashSessionId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_CustomerId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_PaymentSafeId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_PosTerminalId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_WarehouseId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SafeTransactions_CashSessionId",
                table: "SafeTransactions");

            migrationBuilder.DropIndex(
                name: "IX_SafeTransactions_SafeId",
                table: "SafeTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Safes_BranchId",
                table: "Safes");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_BranchId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_PurchaseInvoiceId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_RefundSafeId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_SupplierId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_WarehouseId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_BranchId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_SupplierId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_WarehouseId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_PaymentSafeId",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_PurchaseOrderId",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceItems_TenantId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnits_ProductId_UnitId_TenantId",
                table: "ProductUnits");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnits_TenantId",
                table: "ProductUnits");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnits_UnitId",
                table: "ProductUnits");

            migrationBuilder.DropIndex(
                name: "IX_Products_CategoryId",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_TenantId",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductBarCodes_ProductId",
                table: "ProductBarCodes");

            migrationBuilder.DropIndex(
                name: "IX_PosTerminals_BranchId",
                table: "PosTerminals");

            migrationBuilder.DropIndex(
                name: "IX_PosTerminals_DefaultCashDrawerId",
                table: "PosTerminals");

            migrationBuilder.DropIndex(
                name: "IX_PosDeviceCredentials_PosTerminalId",
                table: "PosDeviceCredentials");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_BranchId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_ExpenseCategoryId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_Expenses_SafeId",
                table: "Expenses");

            migrationBuilder.DropIndex(
                name: "IX_CustomerTransactions_BranchId",
                table: "CustomerTransactions");

            migrationBuilder.DropIndex(
                name: "IX_CustomerTransactions_CustomerId",
                table: "CustomerTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_CashSessions_BranchId",
                table: "CashSessions");

            migrationBuilder.DropIndex(
                name: "IX_CashSessions_PosTerminalId",
                table: "CashSessions");

            migrationBuilder.DropIndex(
                name: "IX_BranchPhones_BranchId",
                table: "BranchPhones");

            migrationBuilder.DropIndex(
                name: "IX_BranchPhones_TenantId_PhoneNumber",
                table: "BranchPhones");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "SubTotal",
                table: "PurchaseInvoices",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "RemainingAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "PaidAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "PurchaseInvoiceItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "PurchaseInvoiceItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "LineTotal",
                table: "PurchaseInvoiceItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "PurchaseInvoiceItems",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "PurchaseInvoiceItemBreakdowns",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AlterColumn<decimal>(
                name: "PackageQuantity",
                table: "PurchaseInvoiceItemBreakdowns",
                type: "decimal(18,4)",
                precision: 18,
                scale: 4,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldPrecision: 18,
                oldScale: 2);

            migrationBuilder.AddCheckConstraint(
                "CK_PurchaseInvoices_Amounts", "PurchaseInvoices",
                "[SubTotal] >= 0 AND [DiscountAmount] >= 0 AND [TaxAmount] >= 0 AND [TotalAmount] >= 0 AND [PaidAmount] >= 0 AND [RemainingAmount] >= 0 AND [PaidAmount] + [RemainingAmount] = [TotalAmount]");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Warehouses_TenantId_Id",
                table: "Warehouses",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Units_TenantId_Id",
                table: "Units",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Suppliers_TenantId_Id",
                table: "Suppliers",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_StockTransfers_TenantId_Id",
                table: "StockTransfers",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_StockAdjustments_TenantId_Id",
                table: "StockAdjustments",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_SalesReturns_TenantId_Id",
                table: "SalesReturns",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_SalesInvoices_TenantId_Id",
                table: "SalesInvoices",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Safes_TenantId_Id",
                table: "Safes",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PurchaseReturns_TenantId_Id",
                table: "PurchaseReturns",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PurchaseOrders_TenantId_Id",
                table: "PurchaseOrders",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PurchaseInvoices_TenantId_Id",
                table: "PurchaseInvoices",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PurchaseInvoiceItems_TenantId_Id",
                table: "PurchaseInvoiceItems",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Products_TenantId_Id",
                table: "Products",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ProductBarCodes_TenantId_Id",
                table: "ProductBarCodes",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ProductBarCodes_TenantId_ProductId_Id",
                table: "ProductBarCodes",
                columns: new[] { "TenantId", "ProductId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_PosTerminals_TenantId_Id",
                table: "PosTerminals",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ExpenseCategories_TenantId_Id",
                table: "ExpenseCategories",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Customers_TenantId_Id",
                table: "Customers",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Categories_TenantId_Id",
                table: "Categories",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_CashSessions_TenantId_Id",
                table: "CashSessions",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Branches_TenantId_Id",
                table: "Branches",
                columns: new[] { "TenantId", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_Units_TenantId_Name",
                table: "Units",
                columns: new[] { "TenantId", "Name" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Units_UnitPackage",
                table: "Units",
                sql: "[UnitPackage] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name",
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPhones_TenantId_PhoneNumber",
                table: "SupplierPhones",
                columns: new[] { "TenantId", "PhoneNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPhones_TenantId_SupplierId",
                table: "SupplierPhones",
                columns: new[] { "TenantId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_StorgeStocks_TenantId_ProductBarcodeId",
                table: "StorgeStocks",
                columns: new[] { "TenantId", "ProductBarcodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_StorgeStocks_TenantId_WarehouseId",
                table: "StorgeStocks",
                columns: new[] { "TenantId", "WarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_StorgeStocks_WarehouseId_ProductBarcodeId",
                table: "StorgeStocks",
                columns: new[] { "WarehouseId", "ProductBarcodeId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferItems_TenantId_ProductId_ProductBarCodeId",
                table: "StockTransferItems",
                columns: new[] { "TenantId", "ProductId", "ProductBarCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockTransferItems_TenantId_StockTransferId",
                table: "StockTransferItems",
                columns: new[] { "TenantId", "StockTransferId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentItems_TenantId_ProductId_ProductBarCodeId",
                table: "StockAdjustmentItems",
                columns: new[] { "TenantId", "ProductId", "ProductBarCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustmentItems_TenantId_StockAdjustmentId",
                table: "StockAdjustmentItems",
                columns: new[] { "TenantId", "StockAdjustmentId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomStocks_TenantId_ProductId",
                table: "ShowroomStocks",
                columns: new[] { "TenantId", "ProductId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomStocks_TenantId_WarehouseId",
                table: "ShowroomStocks",
                columns: new[] { "TenantId", "WarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomStocks_WarehouseId_ProductId",
                table: "ShowroomStocks",
                columns: new[] { "WarehouseId", "ProductId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_TenantId_OriginalInvoiceId",
                table: "SalesReturns",
                columns: new[] { "TenantId", "OriginalInvoiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_TenantId_RefundSafeId",
                table: "SalesReturns",
                columns: new[] { "TenantId", "RefundSafeId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnItems_TenantId_ProductId_ProductBarCodeId",
                table: "SalesReturnItems",
                columns: new[] { "TenantId", "ProductId", "ProductBarCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturnItems_TenantId_SalesReturnId",
                table: "SalesReturnItems",
                columns: new[] { "TenantId", "SalesReturnId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_TenantId_PaymentSafeId",
                table: "SalesInvoices",
                columns: new[] { "TenantId", "PaymentSafeId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_TenantId_PosTerminalId",
                table: "SalesInvoices",
                columns: new[] { "TenantId", "PosTerminalId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceItems_TenantId_ProductId_ProductBarCodeId",
                table: "SalesInvoiceItems",
                columns: new[] { "TenantId", "ProductId", "ProductBarCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoiceItems_TenantId_SalesInvoiceId",
                table: "SalesInvoiceItems",
                columns: new[] { "TenantId", "SalesInvoiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_TenantId_PurchaseInvoiceId",
                table: "PurchaseReturns",
                columns: new[] { "TenantId", "PurchaseInvoiceId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_TenantId_RefundSafeId",
                table: "PurchaseReturns",
                columns: new[] { "TenantId", "RefundSafeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnItems_TenantId_ProductId_ProductBarCodeId",
                table: "PurchaseReturnItems",
                columns: new[] { "TenantId", "ProductId", "ProductBarCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturnItems_TenantId_PurchaseReturnId",
                table: "PurchaseReturnItems",
                columns: new[] { "TenantId", "PurchaseReturnId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseReturnItems_BaseQuantity",
                table: "PurchaseReturnItems",
                sql: "[Quantity] > 0 AND [Quantity] <= 2147483647 AND [Quantity] = FLOOR([Quantity])");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_TenantId_WarehouseId",
                table: "PurchaseOrders",
                columns: new[] { "TenantId", "WarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_TenantId_ProductBarCodeId",
                table: "PurchaseOrderItems",
                columns: new[] { "TenantId", "ProductBarCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrderItems_TenantId_PurchaseOrderId",
                table: "PurchaseOrderItems",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseOrderItems_BaseQuantity",
                table: "PurchaseOrderItems",
                sql: "[Quantity] > 0 AND [Quantity] <= 2147483647 AND [Quantity] = FLOOR([Quantity])");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_TenantId_BranchId",
                table: "PurchaseInvoices",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_TenantId_PaymentSafeId",
                table: "PurchaseInvoices",
                columns: new[] { "TenantId", "PaymentSafeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_TenantId_PurchaseOrderId",
                table: "PurchaseInvoices",
                columns: new[] { "TenantId", "PurchaseOrderId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_TenantId_SupplierId",
                table: "PurchaseInvoices",
                columns: new[] { "TenantId", "SupplierId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_TenantId_WarehouseId",
                table: "PurchaseInvoices",
                columns: new[] { "TenantId", "WarehouseId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceItems_TenantId_ProductId_ProductBarCodeId",
                table: "PurchaseInvoiceItems",
                columns: new[] { "TenantId", "ProductId", "ProductBarCodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceItems_TenantId_PurchaseInvoiceId",
                table: "PurchaseInvoiceItems",
                columns: new[] { "TenantId", "PurchaseInvoiceId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseInvoiceItems_BaseQuantity",
                table: "PurchaseInvoiceItems",
                sql: "[Quantity] > 0 AND [Quantity] <= 2147483647 AND [Quantity] = FLOOR([Quantity])");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceItemBreakdowns_TenantId_ProductBarCodeId",
                table: "PurchaseInvoiceItemBreakdowns",
                columns: new[] { "TenantId", "ProductBarCodeId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseInvoiceItemBreakdowns_BaseQuantity",
                table: "PurchaseInvoiceItemBreakdowns",
                sql: "[Quantity] > 0 AND [Quantity] <= 2147483647 AND [Quantity] = FLOOR([Quantity])");

            migrationBuilder.AddCheckConstraint(
                name: "CK_PurchaseInvoiceItemBreakdowns_Packages",
                table: "PurchaseInvoiceItemBreakdowns",
                sql: "[PackageQuantity] > 0 AND [UnitsPerPackage] > 0 AND [Quantity] = [PackageQuantity] * [UnitsPerPackage]");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_ProductId_UnitId_TenantId",
                table: "ProductUnits",
                columns: new[] { "ProductId", "UnitId", "TenantId" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_TenantId_ProductId",
                table: "ProductUnits",
                columns: new[] { "TenantId", "ProductId" },
                unique: true,
                filter: "[IsDefault] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_TenantId_UnitId",
                table: "ProductUnits",
                columns: new[] { "TenantId", "UnitId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_ProductUnits_ConversionFactor",
                table: "ProductUnits",
                sql: "[ConversionFactor] > 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_TenantId_ProductId",
                table: "ProductImages",
                columns: new[] { "TenantId", "ProductId" },
                unique: true,
                filter: "[IsDefault] = 1 AND [IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_TenantId_ProductId_BarcodeId",
                table: "ProductImages",
                columns: new[] { "TenantId", "ProductId", "BarcodeId" });

            migrationBuilder.CreateIndex(
                name: "IX_PosTerminals_TenantId_DefaultCashDrawerId",
                table: "PosTerminals",
                columns: new[] { "TenantId", "DefaultCashDrawerId" });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerPhones_TenantId_CustomerId",
                table: "CustomerPhones",
                columns: new[] { "TenantId", "CustomerId" });

            migrationBuilder.CreateIndex(
                name: "IX_CashSessions_TenantId_BranchId",
                table: "CashSessions",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_CashSessions_TenantId_CashDrawerId",
                table: "CashSessions",
                columns: new[] { "TenantId", "CashDrawerId" });

            migrationBuilder.CreateIndex(
                name: "IX_CashSessions_TenantId_PosTerminalId",
                table: "CashSessions",
                columns: new[] { "TenantId", "PosTerminalId" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchPhones_TenantId_BranchId",
                table: "BranchPhones",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.CreateIndex(
                name: "IX_BranchPhones_TenantId_PhoneNumber",
                table: "BranchPhones",
                columns: new[] { "TenantId", "PhoneNumber" },
                unique: true,
                filter: "[IsDeleted] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TenantId_BranchId",
                table: "AspNetUsers",
                columns: new[] { "TenantId", "BranchId" });

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Branches_TenantId_BranchId",
                table: "AspNetUsers",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BranchPhones_Branches_TenantId_BranchId",
                table: "BranchPhones",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CashSessions_Branches_TenantId_BranchId",
                table: "CashSessions",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashSessions_PosTerminals_TenantId_PosTerminalId",
                table: "CashSessions",
                columns: new[] { "TenantId", "PosTerminalId" },
                principalTable: "PosTerminals",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashSessions_Safes_TenantId_CashDrawerId",
                table: "CashSessions",
                columns: new[] { "TenantId", "CashDrawerId" },
                principalTable: "Safes",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_TenantId_ParentCategoryId",
                table: "Categories",
                columns: new[] { "TenantId", "ParentCategoryId" },
                principalTable: "Categories",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPhones_Customers_TenantId_CustomerId",
                table: "CustomerPhones",
                columns: new[] { "TenantId", "CustomerId" },
                principalTable: "Customers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerTransactions_Branches_TenantId_BranchId",
                table: "CustomerTransactions",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerTransactions_Customers_TenantId_CustomerId",
                table: "CustomerTransactions",
                columns: new[] { "TenantId", "CustomerId" },
                principalTable: "Customers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Branches_TenantId_BranchId",
                table: "Expenses",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_ExpenseCategories_TenantId_ExpenseCategoryId",
                table: "Expenses",
                columns: new[] { "TenantId", "ExpenseCategoryId" },
                principalTable: "ExpenseCategories",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Safes_TenantId_SafeId",
                table: "Expenses",
                columns: new[] { "TenantId", "SafeId" },
                principalTable: "Safes",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PosDeviceCredentials_PosTerminals_TenantId_PosTerminalId",
                table: "PosDeviceCredentials",
                columns: new[] { "TenantId", "PosTerminalId" },
                principalTable: "PosTerminals",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PosTerminals_Branches_TenantId_BranchId",
                table: "PosTerminals",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PosTerminals_Safes_TenantId_DefaultCashDrawerId",
                table: "PosTerminals",
                columns: new[] { "TenantId", "DefaultCashDrawerId" },
                principalTable: "Safes",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductBarCodes_Products_TenantId_ProductId",
                table: "ProductBarCodes",
                columns: new[] { "TenantId", "ProductId" },
                principalTable: "Products",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductBarCodes_TenantId_ProductId_BarcodeId",
                table: "ProductImages",
                columns: new[] { "TenantId", "ProductId", "BarcodeId" },
                principalTable: "ProductBarCodes",
                principalColumns: new[] { "TenantId", "ProductId", "Id" });

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Products_TenantId_ProductId",
                table: "ProductImages",
                columns: new[] { "TenantId", "ProductId" },
                principalTable: "Products",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_TenantId_CategoryId",
                table: "Products",
                columns: new[] { "TenantId", "CategoryId" },
                principalTable: "Categories",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnits_Products_TenantId_ProductId",
                table: "ProductUnits",
                columns: new[] { "TenantId", "ProductId" },
                principalTable: "Products",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnits_Units_TenantId_UnitId",
                table: "ProductUnits",
                columns: new[] { "TenantId", "UnitId" },
                principalTable: "Units",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItemBreakdowns_ProductBarCodes_TenantId_ProductBarCodeId",
                table: "PurchaseInvoiceItemBreakdowns",
                columns: new[] { "TenantId", "ProductBarCodeId" },
                principalTable: "ProductBarCodes",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItemBreakdowns_PurchaseInvoiceItems_TenantId_PurchaseInvoiceItemId",
                table: "PurchaseInvoiceItemBreakdowns",
                columns: new[] { "TenantId", "PurchaseInvoiceItemId" },
                principalTable: "PurchaseInvoiceItems",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItems_ProductBarCodes_TenantId_ProductId_ProductBarCodeId",
                table: "PurchaseInvoiceItems",
                columns: new[] { "TenantId", "ProductId", "ProductBarCodeId" },
                principalTable: "ProductBarCodes",
                principalColumns: new[] { "TenantId", "ProductId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItems_Products_TenantId_ProductId",
                table: "PurchaseInvoiceItems",
                columns: new[] { "TenantId", "ProductId" },
                principalTable: "Products",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItems_PurchaseInvoices_TenantId_PurchaseInvoiceId",
                table: "PurchaseInvoiceItems",
                columns: new[] { "TenantId", "PurchaseInvoiceId" },
                principalTable: "PurchaseInvoices",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Branches_TenantId_BranchId",
                table: "PurchaseInvoices",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_PurchaseOrders_TenantId_PurchaseOrderId",
                table: "PurchaseInvoices",
                columns: new[] { "TenantId", "PurchaseOrderId" },
                principalTable: "PurchaseOrders",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Safes_TenantId_PaymentSafeId",
                table: "PurchaseInvoices",
                columns: new[] { "TenantId", "PaymentSafeId" },
                principalTable: "Safes",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Suppliers_TenantId_SupplierId",
                table: "PurchaseInvoices",
                columns: new[] { "TenantId", "SupplierId" },
                principalTable: "Suppliers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Warehouses_TenantId_WarehouseId",
                table: "PurchaseInvoices",
                columns: new[] { "TenantId", "WarehouseId" },
                principalTable: "Warehouses",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_ProductBarCodes_TenantId_ProductBarCodeId",
                table: "PurchaseOrderItems",
                columns: new[] { "TenantId", "ProductBarCodeId" },
                principalTable: "ProductBarCodes",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_PurchaseOrders_TenantId_PurchaseOrderId",
                table: "PurchaseOrderItems",
                columns: new[] { "TenantId", "PurchaseOrderId" },
                principalTable: "PurchaseOrders",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Branches_TenantId_BranchId",
                table: "PurchaseOrders",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Suppliers_TenantId_SupplierId",
                table: "PurchaseOrders",
                columns: new[] { "TenantId", "SupplierId" },
                principalTable: "Suppliers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Warehouses_TenantId_WarehouseId",
                table: "PurchaseOrders",
                columns: new[] { "TenantId", "WarehouseId" },
                principalTable: "Warehouses",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturnItems_ProductBarCodes_TenantId_ProductId_ProductBarCodeId",
                table: "PurchaseReturnItems",
                columns: new[] { "TenantId", "ProductId", "ProductBarCodeId" },
                principalTable: "ProductBarCodes",
                principalColumns: new[] { "TenantId", "ProductId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturnItems_Products_TenantId_ProductId",
                table: "PurchaseReturnItems",
                columns: new[] { "TenantId", "ProductId" },
                principalTable: "Products",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturnItems_PurchaseReturns_TenantId_PurchaseReturnId",
                table: "PurchaseReturnItems",
                columns: new[] { "TenantId", "PurchaseReturnId" },
                principalTable: "PurchaseReturns",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Branches_TenantId_BranchId",
                table: "PurchaseReturns",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_PurchaseInvoices_TenantId_PurchaseInvoiceId",
                table: "PurchaseReturns",
                columns: new[] { "TenantId", "PurchaseInvoiceId" },
                principalTable: "PurchaseInvoices",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Safes_TenantId_RefundSafeId",
                table: "PurchaseReturns",
                columns: new[] { "TenantId", "RefundSafeId" },
                principalTable: "Safes",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Suppliers_TenantId_SupplierId",
                table: "PurchaseReturns",
                columns: new[] { "TenantId", "SupplierId" },
                principalTable: "Suppliers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Warehouses_TenantId_WarehouseId",
                table: "PurchaseReturns",
                columns: new[] { "TenantId", "WarehouseId" },
                principalTable: "Warehouses",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Safes_Branches_TenantId_BranchId",
                table: "Safes",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SafeTransactions_CashSessions_TenantId_CashSessionId",
                table: "SafeTransactions",
                columns: new[] { "TenantId", "CashSessionId" },
                principalTable: "CashSessions",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SafeTransactions_Safes_TenantId_SafeId",
                table: "SafeTransactions",
                columns: new[] { "TenantId", "SafeId" },
                principalTable: "Safes",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceItems_ProductBarCodes_TenantId_ProductId_ProductBarCodeId",
                table: "SalesInvoiceItems",
                columns: new[] { "TenantId", "ProductId", "ProductBarCodeId" },
                principalTable: "ProductBarCodes",
                principalColumns: new[] { "TenantId", "ProductId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceItems_Products_TenantId_ProductId",
                table: "SalesInvoiceItems",
                columns: new[] { "TenantId", "ProductId" },
                principalTable: "Products",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceItems_SalesInvoices_TenantId_SalesInvoiceId",
                table: "SalesInvoiceItems",
                columns: new[] { "TenantId", "SalesInvoiceId" },
                principalTable: "SalesInvoices",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_Branches_TenantId_BranchId",
                table: "SalesInvoices",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_CashSessions_TenantId_CashSessionId",
                table: "SalesInvoices",
                columns: new[] { "TenantId", "CashSessionId" },
                principalTable: "CashSessions",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_Customers_TenantId_CustomerId",
                table: "SalesInvoices",
                columns: new[] { "TenantId", "CustomerId" },
                principalTable: "Customers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_PosTerminals_TenantId_PosTerminalId",
                table: "SalesInvoices",
                columns: new[] { "TenantId", "PosTerminalId" },
                principalTable: "PosTerminals",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_Safes_TenantId_PaymentSafeId",
                table: "SalesInvoices",
                columns: new[] { "TenantId", "PaymentSafeId" },
                principalTable: "Safes",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_Warehouses_TenantId_WarehouseId",
                table: "SalesInvoices",
                columns: new[] { "TenantId", "WarehouseId" },
                principalTable: "Warehouses",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturnItems_ProductBarCodes_TenantId_ProductId_ProductBarCodeId",
                table: "SalesReturnItems",
                columns: new[] { "TenantId", "ProductId", "ProductBarCodeId" },
                principalTable: "ProductBarCodes",
                principalColumns: new[] { "TenantId", "ProductId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturnItems_Products_TenantId_ProductId",
                table: "SalesReturnItems",
                columns: new[] { "TenantId", "ProductId" },
                principalTable: "Products",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturnItems_SalesReturns_TenantId_SalesReturnId",
                table: "SalesReturnItems",
                columns: new[] { "TenantId", "SalesReturnId" },
                principalTable: "SalesReturns",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Branches_TenantId_BranchId",
                table: "SalesReturns",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Customers_TenantId_CustomerId",
                table: "SalesReturns",
                columns: new[] { "TenantId", "CustomerId" },
                principalTable: "Customers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Safes_TenantId_RefundSafeId",
                table: "SalesReturns",
                columns: new[] { "TenantId", "RefundSafeId" },
                principalTable: "Safes",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_SalesInvoices_TenantId_OriginalInvoiceId",
                table: "SalesReturns",
                columns: new[] { "TenantId", "OriginalInvoiceId" },
                principalTable: "SalesInvoices",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Warehouses_TenantId_WarehouseId",
                table: "SalesReturns",
                columns: new[] { "TenantId", "WarehouseId" },
                principalTable: "Warehouses",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShowroomStocks_Products_TenantId_ProductId",
                table: "ShowroomStocks",
                columns: new[] { "TenantId", "ProductId" },
                principalTable: "Products",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShowroomStocks_Warehouses_TenantId_WarehouseId",
                table: "ShowroomStocks",
                columns: new[] { "TenantId", "WarehouseId" },
                principalTable: "Warehouses",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAdjustmentItems_ProductBarCodes_TenantId_ProductId_ProductBarCodeId",
                table: "StockAdjustmentItems",
                columns: new[] { "TenantId", "ProductId", "ProductBarCodeId" },
                principalTable: "ProductBarCodes",
                principalColumns: new[] { "TenantId", "ProductId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAdjustmentItems_Products_TenantId_ProductId",
                table: "StockAdjustmentItems",
                columns: new[] { "TenantId", "ProductId" },
                principalTable: "Products",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAdjustmentItems_StockAdjustments_TenantId_StockAdjustmentId",
                table: "StockAdjustmentItems",
                columns: new[] { "TenantId", "StockAdjustmentId" },
                principalTable: "StockAdjustments",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAdjustments_Warehouses_TenantId_WarehouseId",
                table: "StockAdjustments",
                columns: new[] { "TenantId", "WarehouseId" },
                principalTable: "Warehouses",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransferItems_ProductBarCodes_TenantId_ProductId_ProductBarCodeId",
                table: "StockTransferItems",
                columns: new[] { "TenantId", "ProductId", "ProductBarCodeId" },
                principalTable: "ProductBarCodes",
                principalColumns: new[] { "TenantId", "ProductId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransferItems_Products_TenantId_ProductId",
                table: "StockTransferItems",
                columns: new[] { "TenantId", "ProductId" },
                principalTable: "Products",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransferItems_StockTransfers_TenantId_StockTransferId",
                table: "StockTransferItems",
                columns: new[] { "TenantId", "StockTransferId" },
                principalTable: "StockTransfers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransfers_Warehouses_TenantId_FromWarehouseId",
                table: "StockTransfers",
                columns: new[] { "TenantId", "FromWarehouseId" },
                principalTable: "Warehouses",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransfers_Warehouses_TenantId_ToWarehouseId",
                table: "StockTransfers",
                columns: new[] { "TenantId", "ToWarehouseId" },
                principalTable: "Warehouses",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorgeStocks_ProductBarCodes_TenantId_ProductBarcodeId",
                table: "StorgeStocks",
                columns: new[] { "TenantId", "ProductBarcodeId" },
                principalTable: "ProductBarCodes",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorgeStocks_Warehouses_TenantId_WarehouseId",
                table: "StorgeStocks",
                columns: new[] { "TenantId", "WarehouseId" },
                principalTable: "Warehouses",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierPhones_Suppliers_TenantId_SupplierId",
                table: "SupplierPhones",
                columns: new[] { "TenantId", "SupplierId" },
                principalTable: "Suppliers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierTransactions_Branches_TenantId_BranchId",
                table: "SupplierTransactions",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierTransactions_Suppliers_TenantId_SupplierId",
                table: "SupplierTransactions",
                columns: new[] { "TenantId", "SupplierId" },
                principalTable: "Suppliers",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Branches_TenantId_BranchId",
                table: "Warehouses",
                columns: new[] { "TenantId", "BranchId" },
                principalTable: "Branches",
                principalColumns: new[] { "TenantId", "Id" },
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint("CK_PurchaseInvoices_Amounts", "PurchaseInvoices");
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Branches_TenantId_BranchId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_BranchPhones_Branches_TenantId_BranchId",
                table: "BranchPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_CashSessions_Branches_TenantId_BranchId",
                table: "CashSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_CashSessions_PosTerminals_TenantId_PosTerminalId",
                table: "CashSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_CashSessions_Safes_TenantId_CashDrawerId",
                table: "CashSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Categories_Categories_TenantId_ParentCategoryId",
                table: "Categories");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerPhones_Customers_TenantId_CustomerId",
                table: "CustomerPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerTransactions_Branches_TenantId_BranchId",
                table: "CustomerTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_CustomerTransactions_Customers_TenantId_CustomerId",
                table: "CustomerTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Branches_TenantId_BranchId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_ExpenseCategories_TenantId_ExpenseCategoryId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_Expenses_Safes_TenantId_SafeId",
                table: "Expenses");

            migrationBuilder.DropForeignKey(
                name: "FK_PosDeviceCredentials_PosTerminals_TenantId_PosTerminalId",
                table: "PosDeviceCredentials");

            migrationBuilder.DropForeignKey(
                name: "FK_PosTerminals_Branches_TenantId_BranchId",
                table: "PosTerminals");

            migrationBuilder.DropForeignKey(
                name: "FK_PosTerminals_Safes_TenantId_DefaultCashDrawerId",
                table: "PosTerminals");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductBarCodes_Products_TenantId_ProductId",
                table: "ProductBarCodes");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_ProductBarCodes_TenantId_ProductId_BarcodeId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Products_TenantId_ProductId",
                table: "ProductImages");

            migrationBuilder.DropForeignKey(
                name: "FK_Products_Categories_TenantId_CategoryId",
                table: "Products");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnits_Products_TenantId_ProductId",
                table: "ProductUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductUnits_Units_TenantId_UnitId",
                table: "ProductUnits");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItemBreakdowns_ProductBarCodes_TenantId_ProductBarCodeId",
                table: "PurchaseInvoiceItemBreakdowns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItemBreakdowns_PurchaseInvoiceItems_TenantId_PurchaseInvoiceItemId",
                table: "PurchaseInvoiceItemBreakdowns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItems_ProductBarCodes_TenantId_ProductId_ProductBarCodeId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItems_Products_TenantId_ProductId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoiceItems_PurchaseInvoices_TenantId_PurchaseInvoiceId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Branches_TenantId_BranchId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_PurchaseOrders_TenantId_PurchaseOrderId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Safes_TenantId_PaymentSafeId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Suppliers_TenantId_SupplierId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseInvoices_Warehouses_TenantId_WarehouseId",
                table: "PurchaseInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_ProductBarCodes_TenantId_ProductBarCodeId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrderItems_PurchaseOrders_TenantId_PurchaseOrderId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Branches_TenantId_BranchId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Suppliers_TenantId_SupplierId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseOrders_Warehouses_TenantId_WarehouseId",
                table: "PurchaseOrders");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturnItems_ProductBarCodes_TenantId_ProductId_ProductBarCodeId",
                table: "PurchaseReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturnItems_Products_TenantId_ProductId",
                table: "PurchaseReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturnItems_PurchaseReturns_TenantId_PurchaseReturnId",
                table: "PurchaseReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Branches_TenantId_BranchId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_PurchaseInvoices_TenantId_PurchaseInvoiceId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Safes_TenantId_RefundSafeId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Suppliers_TenantId_SupplierId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_PurchaseReturns_Warehouses_TenantId_WarehouseId",
                table: "PurchaseReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_Safes_Branches_TenantId_BranchId",
                table: "Safes");

            migrationBuilder.DropForeignKey(
                name: "FK_SafeTransactions_CashSessions_TenantId_CashSessionId",
                table: "SafeTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SafeTransactions_Safes_TenantId_SafeId",
                table: "SafeTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceItems_ProductBarCodes_TenantId_ProductId_ProductBarCodeId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceItems_Products_TenantId_ProductId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoiceItems_SalesInvoices_TenantId_SalesInvoiceId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Branches_TenantId_BranchId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_CashSessions_TenantId_CashSessionId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Customers_TenantId_CustomerId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_PosTerminals_TenantId_PosTerminalId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Safes_TenantId_PaymentSafeId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesInvoices_Warehouses_TenantId_WarehouseId",
                table: "SalesInvoices");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturnItems_ProductBarCodes_TenantId_ProductId_ProductBarCodeId",
                table: "SalesReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturnItems_Products_TenantId_ProductId",
                table: "SalesReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturnItems_SalesReturns_TenantId_SalesReturnId",
                table: "SalesReturnItems");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Branches_TenantId_BranchId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Customers_TenantId_CustomerId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Safes_TenantId_RefundSafeId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_SalesInvoices_TenantId_OriginalInvoiceId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_SalesReturns_Warehouses_TenantId_WarehouseId",
                table: "SalesReturns");

            migrationBuilder.DropForeignKey(
                name: "FK_ShowroomStocks_Products_TenantId_ProductId",
                table: "ShowroomStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_ShowroomStocks_Warehouses_TenantId_WarehouseId",
                table: "ShowroomStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAdjustmentItems_ProductBarCodes_TenantId_ProductId_ProductBarCodeId",
                table: "StockAdjustmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAdjustmentItems_Products_TenantId_ProductId",
                table: "StockAdjustmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAdjustmentItems_StockAdjustments_TenantId_StockAdjustmentId",
                table: "StockAdjustmentItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockAdjustments_Warehouses_TenantId_WarehouseId",
                table: "StockAdjustments");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransferItems_ProductBarCodes_TenantId_ProductId_ProductBarCodeId",
                table: "StockTransferItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransferItems_Products_TenantId_ProductId",
                table: "StockTransferItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransferItems_StockTransfers_TenantId_StockTransferId",
                table: "StockTransferItems");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransfers_Warehouses_TenantId_FromWarehouseId",
                table: "StockTransfers");

            migrationBuilder.DropForeignKey(
                name: "FK_StockTransfers_Warehouses_TenantId_ToWarehouseId",
                table: "StockTransfers");

            migrationBuilder.DropForeignKey(
                name: "FK_StorgeStocks_ProductBarCodes_TenantId_ProductBarcodeId",
                table: "StorgeStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_StorgeStocks_Warehouses_TenantId_WarehouseId",
                table: "StorgeStocks");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierPhones_Suppliers_TenantId_SupplierId",
                table: "SupplierPhones");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierTransactions_Branches_TenantId_BranchId",
                table: "SupplierTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_SupplierTransactions_Suppliers_TenantId_SupplierId",
                table: "SupplierTransactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Warehouses_Branches_TenantId_BranchId",
                table: "Warehouses");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Warehouses_TenantId_Id",
                table: "Warehouses");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Units_TenantId_Id",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Units_TenantId_Name",
                table: "Units");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Units_UnitPackage",
                table: "Units");

            migrationBuilder.DropIndex(
                name: "IX_Tenants_Name",
                table: "Tenants");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Suppliers_TenantId_Id",
                table: "Suppliers");

            migrationBuilder.DropIndex(
                name: "IX_SupplierPhones_TenantId_PhoneNumber",
                table: "SupplierPhones");

            migrationBuilder.DropIndex(
                name: "IX_SupplierPhones_TenantId_SupplierId",
                table: "SupplierPhones");

            migrationBuilder.DropIndex(
                name: "IX_StorgeStocks_TenantId_ProductBarcodeId",
                table: "StorgeStocks");

            migrationBuilder.DropIndex(
                name: "IX_StorgeStocks_TenantId_WarehouseId",
                table: "StorgeStocks");

            migrationBuilder.DropIndex(
                name: "IX_StorgeStocks_WarehouseId_ProductBarcodeId",
                table: "StorgeStocks");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_StockTransfers_TenantId_Id",
                table: "StockTransfers");

            migrationBuilder.DropIndex(
                name: "IX_StockTransferItems_TenantId_ProductId_ProductBarCodeId",
                table: "StockTransferItems");

            migrationBuilder.DropIndex(
                name: "IX_StockTransferItems_TenantId_StockTransferId",
                table: "StockTransferItems");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_StockAdjustments_TenantId_Id",
                table: "StockAdjustments");

            migrationBuilder.DropIndex(
                name: "IX_StockAdjustmentItems_TenantId_ProductId_ProductBarCodeId",
                table: "StockAdjustmentItems");

            migrationBuilder.DropIndex(
                name: "IX_StockAdjustmentItems_TenantId_StockAdjustmentId",
                table: "StockAdjustmentItems");

            migrationBuilder.DropIndex(
                name: "IX_ShowroomStocks_TenantId_ProductId",
                table: "ShowroomStocks");

            migrationBuilder.DropIndex(
                name: "IX_ShowroomStocks_TenantId_WarehouseId",
                table: "ShowroomStocks");

            migrationBuilder.DropIndex(
                name: "IX_ShowroomStocks_WarehouseId_ProductId",
                table: "ShowroomStocks");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_SalesReturns_TenantId_Id",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_TenantId_OriginalInvoiceId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturns_TenantId_RefundSafeId",
                table: "SalesReturns");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturnItems_TenantId_ProductId_ProductBarCodeId",
                table: "SalesReturnItems");

            migrationBuilder.DropIndex(
                name: "IX_SalesReturnItems_TenantId_SalesReturnId",
                table: "SalesReturnItems");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_SalesInvoices_TenantId_Id",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_TenantId_PaymentSafeId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoices_TenantId_PosTerminalId",
                table: "SalesInvoices");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceItems_TenantId_ProductId_ProductBarCodeId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_SalesInvoiceItems_TenantId_SalesInvoiceId",
                table: "SalesInvoiceItems");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Safes_TenantId_Id",
                table: "Safes");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PurchaseReturns_TenantId_Id",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_TenantId_PurchaseInvoiceId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturns_TenantId_RefundSafeId",
                table: "PurchaseReturns");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturnItems_TenantId_ProductId_ProductBarCodeId",
                table: "PurchaseReturnItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseReturnItems_TenantId_PurchaseReturnId",
                table: "PurchaseReturnItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseReturnItems_BaseQuantity",
                table: "PurchaseReturnItems");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PurchaseOrders_TenantId_Id",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrders_TenantId_WarehouseId",
                table: "PurchaseOrders");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_TenantId_ProductBarCodeId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseOrderItems_TenantId_PurchaseOrderId",
                table: "PurchaseOrderItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseOrderItems_BaseQuantity",
                table: "PurchaseOrderItems");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PurchaseInvoices_TenantId_Id",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_TenantId_BranchId",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_TenantId_PaymentSafeId",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_TenantId_PurchaseOrderId",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_TenantId_SupplierId",
                table: "PurchaseInvoices");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoices_TenantId_WarehouseId",
                table: "PurchaseInvoices");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PurchaseInvoiceItems_TenantId_Id",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceItems_TenantId_ProductId_ProductBarCodeId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceItems_TenantId_PurchaseInvoiceId",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseInvoiceItems_BaseQuantity",
                table: "PurchaseInvoiceItems");

            migrationBuilder.DropIndex(
                name: "IX_PurchaseInvoiceItemBreakdowns_TenantId_ProductBarCodeId",
                table: "PurchaseInvoiceItemBreakdowns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseInvoiceItemBreakdowns_BaseQuantity",
                table: "PurchaseInvoiceItemBreakdowns");

            migrationBuilder.DropCheckConstraint(
                name: "CK_PurchaseInvoiceItemBreakdowns_Packages",
                table: "PurchaseInvoiceItemBreakdowns");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnits_ProductId_UnitId_TenantId",
                table: "ProductUnits");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnits_TenantId_ProductId",
                table: "ProductUnits");

            migrationBuilder.DropIndex(
                name: "IX_ProductUnits_TenantId_UnitId",
                table: "ProductUnits");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ProductUnits_ConversionFactor",
                table: "ProductUnits");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Products_TenantId_Id",
                table: "Products");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_TenantId_ProductId",
                table: "ProductImages");

            migrationBuilder.DropIndex(
                name: "IX_ProductImages_TenantId_ProductId_BarcodeId",
                table: "ProductImages");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ProductBarCodes_TenantId_Id",
                table: "ProductBarCodes");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ProductBarCodes_TenantId_ProductId_Id",
                table: "ProductBarCodes");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_PosTerminals_TenantId_Id",
                table: "PosTerminals");

            migrationBuilder.DropIndex(
                name: "IX_PosTerminals_TenantId_DefaultCashDrawerId",
                table: "PosTerminals");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ExpenseCategories_TenantId_Id",
                table: "ExpenseCategories");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Customers_TenantId_Id",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_CustomerPhones_TenantId_CustomerId",
                table: "CustomerPhones");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Categories_TenantId_Id",
                table: "Categories");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_CashSessions_TenantId_Id",
                table: "CashSessions");

            migrationBuilder.DropIndex(
                name: "IX_CashSessions_TenantId_BranchId",
                table: "CashSessions");

            migrationBuilder.DropIndex(
                name: "IX_CashSessions_TenantId_CashDrawerId",
                table: "CashSessions");

            migrationBuilder.DropIndex(
                name: "IX_CashSessions_TenantId_PosTerminalId",
                table: "CashSessions");

            migrationBuilder.DropIndex(
                name: "IX_BranchPhones_TenantId_BranchId",
                table: "BranchPhones");

            migrationBuilder.DropIndex(
                name: "IX_BranchPhones_TenantId_PhoneNumber",
                table: "BranchPhones");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Branches_TenantId_Id",
                table: "Branches");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_TenantId_BranchId",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "TaxAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "SubTotal",
                table: "PurchaseInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "RemainingAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "PaidAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "PurchaseInvoices",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitPrice",
                table: "PurchaseInvoiceItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "PurchaseInvoiceItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "LineTotal",
                table: "PurchaseInvoiceItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "DiscountAmount",
                table: "PurchaseInvoiceItems",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "Quantity",
                table: "PurchaseInvoiceItemBreakdowns",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AlterColumn<decimal>(
                name: "PackageQuantity",
                table: "PurchaseInvoiceItemBreakdowns",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,4)",
                oldPrecision: 18,
                oldScale: 4);

            migrationBuilder.AddCheckConstraint(
                "CK_PurchaseInvoices_Amounts", "PurchaseInvoices",
                "[SubTotal] >= 0 AND [DiscountAmount] >= 0 AND [TaxAmount] >= 0 AND [TotalAmount] >= 0 AND [PaidAmount] >= 0 AND [RemainingAmount] >= 0 AND [PaidAmount] + [RemainingAmount] = [TotalAmount]");

            migrationBuilder.CreateIndex(
                name: "IX_Warehouses_BranchId",
                table: "Warehouses",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Units_TenantId_Name",
                table: "Units",
                columns: new[] { "TenantId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupplierTransactions_BranchId",
                table: "SupplierTransactions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierTransactions_SupplierId",
                table: "SupplierTransactions",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_SupplierPhones_TenantId_PhoneNumber",
                table: "SupplierPhones",
                columns: new[] { "TenantId", "PhoneNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StorgeStocks_ProductBarcodeId",
                table: "StorgeStocks",
                column: "ProductBarcodeId");

            migrationBuilder.CreateIndex(
                name: "IX_StorgeStocks_WarehouseId_ProductBarcodeId",
                table: "StorgeStocks",
                columns: new[] { "WarehouseId", "ProductBarcodeId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_FromWarehouseId",
                table: "StockTransfers",
                column: "FromWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockTransfers_ToWarehouseId",
                table: "StockTransfers",
                column: "ToWarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_StockAdjustments_WarehouseId",
                table: "StockAdjustments",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomStocks_ProductId",
                table: "ShowroomStocks",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_ShowroomStocks_WarehouseId_ProductId",
                table: "ShowroomStocks",
                columns: new[] { "WarehouseId", "ProductId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_BranchId",
                table: "SalesReturns",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_CustomerId",
                table: "SalesReturns",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_OriginalInvoiceId",
                table: "SalesReturns",
                column: "OriginalInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_RefundSafeId",
                table: "SalesReturns",
                column: "RefundSafeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesReturns_WarehouseId",
                table: "SalesReturns",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_BranchId",
                table: "SalesInvoices",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_CashSessionId",
                table: "SalesInvoices",
                column: "CashSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_CustomerId",
                table: "SalesInvoices",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_PaymentSafeId",
                table: "SalesInvoices",
                column: "PaymentSafeId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_PosTerminalId",
                table: "SalesInvoices",
                column: "PosTerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_SalesInvoices_WarehouseId",
                table: "SalesInvoices",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeTransactions_CashSessionId",
                table: "SafeTransactions",
                column: "CashSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SafeTransactions_SafeId",
                table: "SafeTransactions",
                column: "SafeId");

            migrationBuilder.CreateIndex(
                name: "IX_Safes_BranchId",
                table: "Safes",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_BranchId",
                table: "PurchaseReturns",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_PurchaseInvoiceId",
                table: "PurchaseReturns",
                column: "PurchaseInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_RefundSafeId",
                table: "PurchaseReturns",
                column: "RefundSafeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_SupplierId",
                table: "PurchaseReturns",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseReturns_WarehouseId",
                table: "PurchaseReturns",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_BranchId",
                table: "PurchaseOrders",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_WarehouseId",
                table: "PurchaseOrders",
                column: "WarehouseId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_PaymentSafeId",
                table: "PurchaseInvoices",
                column: "PaymentSafeId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoices_PurchaseOrderId",
                table: "PurchaseInvoices",
                column: "PurchaseOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseInvoiceItems_TenantId",
                table: "PurchaseInvoiceItems",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_ProductId_UnitId_TenantId",
                table: "ProductUnits",
                columns: new[] { "ProductId", "UnitId", "TenantId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_TenantId",
                table: "ProductUnits",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductUnits_UnitId",
                table: "ProductUnits",
                column: "UnitId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductImages_TenantId",
                table: "ProductImages",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductBarCodes_ProductId",
                table: "ProductBarCodes",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_PosTerminals_BranchId",
                table: "PosTerminals",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_PosTerminals_DefaultCashDrawerId",
                table: "PosTerminals",
                column: "DefaultCashDrawerId");

            migrationBuilder.CreateIndex(
                name: "IX_PosDeviceCredentials_PosTerminalId",
                table: "PosDeviceCredentials",
                column: "PosTerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_BranchId",
                table: "Expenses",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_ExpenseCategoryId",
                table: "Expenses",
                column: "ExpenseCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Expenses_SafeId",
                table: "Expenses",
                column: "SafeId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTransactions_BranchId",
                table: "CustomerTransactions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerTransactions_CustomerId",
                table: "CustomerTransactions",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CashSessions_BranchId",
                table: "CashSessions",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_CashSessions_PosTerminalId",
                table: "CashSessions",
                column: "PosTerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchPhones_BranchId",
                table: "BranchPhones",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchPhones_TenantId_PhoneNumber",
                table: "BranchPhones",
                columns: new[] { "TenantId", "PhoneNumber" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Branches_BranchId",
                table: "AspNetUsers",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_BranchPhones_Branches_BranchId",
                table: "BranchPhones",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CashSessions_Branches_BranchId",
                table: "CashSessions",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashSessions_PosTerminals_PosTerminalId",
                table: "CashSessions",
                column: "PosTerminalId",
                principalTable: "PosTerminals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CashSessions_Safes_CashDrawerId",
                table: "CashSessions",
                column: "CashDrawerId",
                principalTable: "Safes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Categories_Categories_ParentCategoryId",
                table: "Categories",
                column: "ParentCategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerPhones_Customers_CustomerId",
                table: "CustomerPhones",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerTransactions_Branches_BranchId",
                table: "CustomerTransactions",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CustomerTransactions_Customers_CustomerId",
                table: "CustomerTransactions",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Branches_BranchId",
                table: "Expenses",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_ExpenseCategories_ExpenseCategoryId",
                table: "Expenses",
                column: "ExpenseCategoryId",
                principalTable: "ExpenseCategories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Expenses_Safes_SafeId",
                table: "Expenses",
                column: "SafeId",
                principalTable: "Safes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PosDeviceCredentials_PosTerminals_PosTerminalId",
                table: "PosDeviceCredentials",
                column: "PosTerminalId",
                principalTable: "PosTerminals",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PosTerminals_Branches_BranchId",
                table: "PosTerminals",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PosTerminals_Safes_DefaultCashDrawerId",
                table: "PosTerminals",
                column: "DefaultCashDrawerId",
                principalTable: "Safes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductBarCodes_Products_ProductId",
                table: "ProductBarCodes",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_ProductBarCodes_BarcodeId",
                table: "ProductImages",
                column: "BarcodeId",
                principalTable: "ProductBarCodes",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Categories_CategoryId",
                table: "Products",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnits_Products_ProductId",
                table: "ProductUnits",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductUnits_Units_UnitId",
                table: "ProductUnits",
                column: "UnitId",
                principalTable: "Units",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItemBreakdowns_ProductBarCodes_ProductBarCodeId",
                table: "PurchaseInvoiceItemBreakdowns",
                column: "ProductBarCodeId",
                principalTable: "ProductBarCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItemBreakdowns_PurchaseInvoiceItems_PurchaseInvoiceItemId",
                table: "PurchaseInvoiceItemBreakdowns",
                column: "PurchaseInvoiceItemId",
                principalTable: "PurchaseInvoiceItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItems_ProductBarCodes_ProductBarCodeId",
                table: "PurchaseInvoiceItems",
                column: "ProductBarCodeId",
                principalTable: "ProductBarCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItems_Products_ProductId",
                table: "PurchaseInvoiceItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoiceItems_PurchaseInvoices_PurchaseInvoiceId",
                table: "PurchaseInvoiceItems",
                column: "PurchaseInvoiceId",
                principalTable: "PurchaseInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Branches_BranchId",
                table: "PurchaseInvoices",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseInvoices",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Safes_PaymentSafeId",
                table: "PurchaseInvoices",
                column: "PaymentSafeId",
                principalTable: "Safes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Suppliers_SupplierId",
                table: "PurchaseInvoices",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseInvoices_Warehouses_WarehouseId",
                table: "PurchaseInvoices",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_ProductBarCodes_ProductBarCodeId",
                table: "PurchaseOrderItems",
                column: "ProductBarCodeId",
                principalTable: "ProductBarCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrderItems_PurchaseOrders_PurchaseOrderId",
                table: "PurchaseOrderItems",
                column: "PurchaseOrderId",
                principalTable: "PurchaseOrders",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Branches_BranchId",
                table: "PurchaseOrders",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Suppliers_SupplierId",
                table: "PurchaseOrders",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseOrders_Warehouses_WarehouseId",
                table: "PurchaseOrders",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturnItems_ProductBarCodes_ProductBarCodeId",
                table: "PurchaseReturnItems",
                column: "ProductBarCodeId",
                principalTable: "ProductBarCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturnItems_Products_ProductId",
                table: "PurchaseReturnItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturnItems_PurchaseReturns_PurchaseReturnId",
                table: "PurchaseReturnItems",
                column: "PurchaseReturnId",
                principalTable: "PurchaseReturns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Branches_BranchId",
                table: "PurchaseReturns",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_PurchaseInvoices_PurchaseInvoiceId",
                table: "PurchaseReturns",
                column: "PurchaseInvoiceId",
                principalTable: "PurchaseInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Safes_RefundSafeId",
                table: "PurchaseReturns",
                column: "RefundSafeId",
                principalTable: "Safes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Suppliers_SupplierId",
                table: "PurchaseReturns",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PurchaseReturns_Warehouses_WarehouseId",
                table: "PurchaseReturns",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Safes_Branches_BranchId",
                table: "Safes",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SafeTransactions_CashSessions_CashSessionId",
                table: "SafeTransactions",
                column: "CashSessionId",
                principalTable: "CashSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SafeTransactions_Safes_SafeId",
                table: "SafeTransactions",
                column: "SafeId",
                principalTable: "Safes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceItems_ProductBarCodes_ProductBarCodeId",
                table: "SalesInvoiceItems",
                column: "ProductBarCodeId",
                principalTable: "ProductBarCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceItems_Products_ProductId",
                table: "SalesInvoiceItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoiceItems_SalesInvoices_SalesInvoiceId",
                table: "SalesInvoiceItems",
                column: "SalesInvoiceId",
                principalTable: "SalesInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_Branches_BranchId",
                table: "SalesInvoices",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_CashSessions_CashSessionId",
                table: "SalesInvoices",
                column: "CashSessionId",
                principalTable: "CashSessions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_Customers_CustomerId",
                table: "SalesInvoices",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesInvoices_PosTerminals_PosTerminalId",
                table: "SalesInvoices",
                column: "PosTerminalId",
                principalTable: "PosTerminals",
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
                name: "FK_SalesInvoices_Warehouses_WarehouseId",
                table: "SalesInvoices",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturnItems_ProductBarCodes_ProductBarCodeId",
                table: "SalesReturnItems",
                column: "ProductBarCodeId",
                principalTable: "ProductBarCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturnItems_Products_ProductId",
                table: "SalesReturnItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturnItems_SalesReturns_SalesReturnId",
                table: "SalesReturnItems",
                column: "SalesReturnId",
                principalTable: "SalesReturns",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Branches_BranchId",
                table: "SalesReturns",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Customers_CustomerId",
                table: "SalesReturns",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Safes_RefundSafeId",
                table: "SalesReturns",
                column: "RefundSafeId",
                principalTable: "Safes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_SalesInvoices_OriginalInvoiceId",
                table: "SalesReturns",
                column: "OriginalInvoiceId",
                principalTable: "SalesInvoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_SalesReturns_Warehouses_WarehouseId",
                table: "SalesReturns",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShowroomStocks_Products_ProductId",
                table: "ShowroomStocks",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ShowroomStocks_Warehouses_WarehouseId",
                table: "ShowroomStocks",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAdjustmentItems_ProductBarCodes_ProductBarCodeId",
                table: "StockAdjustmentItems",
                column: "ProductBarCodeId",
                principalTable: "ProductBarCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAdjustmentItems_Products_ProductId",
                table: "StockAdjustmentItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAdjustmentItems_StockAdjustments_StockAdjustmentId",
                table: "StockAdjustmentItems",
                column: "StockAdjustmentId",
                principalTable: "StockAdjustments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockAdjustments_Warehouses_WarehouseId",
                table: "StockAdjustments",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransferItems_ProductBarCodes_ProductBarCodeId",
                table: "StockTransferItems",
                column: "ProductBarCodeId",
                principalTable: "ProductBarCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransferItems_Products_ProductId",
                table: "StockTransferItems",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransferItems_StockTransfers_StockTransferId",
                table: "StockTransferItems",
                column: "StockTransferId",
                principalTable: "StockTransfers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransfers_Warehouses_FromWarehouseId",
                table: "StockTransfers",
                column: "FromWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StockTransfers_Warehouses_ToWarehouseId",
                table: "StockTransfers",
                column: "ToWarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorgeStocks_ProductBarCodes_ProductBarcodeId",
                table: "StorgeStocks",
                column: "ProductBarcodeId",
                principalTable: "ProductBarCodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_StorgeStocks_Warehouses_WarehouseId",
                table: "StorgeStocks",
                column: "WarehouseId",
                principalTable: "Warehouses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierPhones_Suppliers_SupplierId",
                table: "SupplierPhones",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierTransactions_Branches_BranchId",
                table: "SupplierTransactions",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SupplierTransactions_Suppliers_SupplierId",
                table: "SupplierTransactions",
                column: "SupplierId",
                principalTable: "Suppliers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Warehouses_Branches_BranchId",
                table: "Warehouses",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
