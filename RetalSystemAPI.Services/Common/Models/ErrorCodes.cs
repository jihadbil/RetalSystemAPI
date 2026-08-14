namespace RetalSystemAPI.Services.Common.Models;

/// <summary>
/// ثوابت موحدة لأكواد الأخطاء في النظام.
/// </summary>
public static class ErrorCodes
{
    // ── عامة ──────────────────────────────────────────────
    public const string NotFound = "NOT_FOUND";
    public const string Unauthorized = "UNAUTHORIZED";
    public const string ValidationError = "VALIDATION_ERROR";
    public const string ConcurrencyError = "CONCURRENCY_ERROR";

    // ── Tenant ─────────────────────────────────────────────
    public const string TenantNotFound = "TENANT_NOT_FOUND";
    public const string TenantNameExists = "TENANT_NAME_EXISTS";

    // ── Auth & Users ───────────────────────────────────────
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string UserAlreadyExists = "USER_ALREADY_EXISTS";
    public const string UserNotFound = "USER_NOT_FOUND";

    // ── Branch ─────────────────────────────────────────────
    public const string BranchNotFound = "BRANCH_NOT_FOUND";
    public const string BranchNameExists = "BRANCH_NAME_EXISTS";
    public const string BranchHasUsers = "BRANCH_HAS_ACTIVE_USERS";

    // ── Catalog ────────────────────────────────────────────
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";
    public const string CategoryHasProducts = "CATEGORY_HAS_PRODUCTS";
    public const string CategoryCircularRef = "CATEGORY_CIRCULAR_REFERENCE";
    public const string CategoryNameExists = "CATEGORY_NAME_EXISTS";

    public const string ProductNotFound = "PRODUCT_NOT_FOUND";

    public const string UnitNotFound = "UNIT_NOT_FOUND";
    public const string UnitNameExists = "UNIT_NAME_EXISTS";
    public const string UnitInUse = "UNIT_IN_USE";

    public const string ProductUnitNotFound = "PRODUCT_UNIT_NOT_FOUND";
    public const string ProductUnitDuplicate = "PRODUCT_UNIT_DUPLICATE";

    public const string BarCodeNotFound = "BARCODE_NOT_FOUND";
    public const string BarCodeDuplicate = "BARCODE_DUPLICATE";

    public const string ImageNotFound = "IMAGE_NOT_FOUND";

    // ── File Upload ────────────────────────────────────────
    public const string InvalidFileType = "INVALID_FILE_TYPE";
    public const string FileTooLarge = "FILE_TOO_LARGE";
    public const string UploadFailed = "UPLOAD_FAILED";

    // ── Suppliers ──────────────────────────────────────────
    public const string SupplierNotFound = "SUPPLIER_NOT_FOUND";
    public const string SupplierNameExists = "SUPPLIER_NAME_EXISTS";

    // ── Warehouses ─────────────────────────────────────────
    public const string WarehouseNotFound = "WAREHOUSE_NOT_FOUND";
    public const string WarehouseNameExists = "WAREHOUSE_NAME_EXISTS";
    public const string WarehouseHasStock = "WAREHOUSE_HAS_STOCK";

    // ── Stock ──────────────────────────────────────────────
    public const string StockNotFound = "STOCK_NOT_FOUND";
    public const string InsufficientStock = "INSUFFICIENT_STOCK";

    // ── Purchase Orders ────────────────────────────────────
    public const string PurchaseOrderNotFound = "PURCHASE_ORDER_NOT_FOUND";
    public const string PurchaseOrderNumberExists = "PURCHASE_ORDER_NUMBER_EXISTS";
    public const string PurchaseOrderInvalidStatus = "PURCHASE_ORDER_INVALID_STATUS";
}
