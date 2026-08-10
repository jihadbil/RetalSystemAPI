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

    // ── Auth ───────────────────────────────────────────────
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string UserAlreadyExists = "USER_ALREADY_EXISTS";

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
}
