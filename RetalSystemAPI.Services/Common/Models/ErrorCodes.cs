namespace RetalSystemAPI.Services.Common.Models;

/// <summary>
/// ثوابت موحدة لرموز وأكواد الأخطاء على مستوى النظام لتوحيد الاستجابات بين الواجهات البرمجية والعملاء.
/// </summary>
public static class ErrorCodes
{
    // ── عامة ──────────────────────────────────────────────
    /// <summary>لم يتم العثور على العنصر أو المورد المطلوب.</summary>
    public const string NotFound = "NOT_FOUND";

    /// <summary>المستخدم غير مصادق عليه أو الجلسة غير صالحة.</summary>
    public const string Unauthorized = "UNAUTHORIZED";

    /// <summary>المستخدم لا يملك الصلاحية اللازمة للقيام بهذه العملية.</summary>
    public const string Forbidden = "FORBIDDEN";

    /// <summary>خطأ في التحقق من صحة المدخلات والبيانات المرسلة.</summary>
    public const string ValidationError = "VALIDATION_ERROR";

    /// <summary>تعارض في البيانات المتزامنة (Concurrency Conflict).</summary>
    public const string ConcurrencyError = "CONCURRENCY_ERROR";

    // ── Tenant (المستأجر) ──────────────────────────────────
    /// <summary>المستأجر المحدد غير موجود.</summary>
    public const string TenantNotFound = "TENANT_NOT_FOUND";

    /// <summary>اسم المستأجر مسجل مسبقاً لمستأجر آخر.</summary>
    public const string TenantNameExists = "TENANT_NAME_EXISTS";

    // ── Auth & Users (المصادقة والمستخدمين) ─────────────────
    /// <summary>بيانات الاعتماد غير صالحة (اسم المستخدم أو كلمة المرور غير صحيحة).</summary>
    public const string InvalidCredentials = "INVALID_CREDENTIALS";

    /// <summary>المستخدم أو البريد الإلكتروني مسجل مسبقاً.</summary>
    public const string UserAlreadyExists = "USER_ALREADY_EXISTS";

    /// <summary>المستخدم المحدد غير موجود في المنظومة.</summary>
    public const string UserNotFound = "USER_NOT_FOUND";

    // ── Branch (الفروع) ────────────────────────────────────
    /// <summary>الفرع المحدد غير موجود.</summary>
    public const string BranchNotFound = "BRANCH_NOT_FOUND";

    /// <summary>اسم الفرع مستخدم مسبقاً لنفس المستأجر.</summary>
    public const string BranchNameExists = "BRANCH_NAME_EXISTS";

    /// <summary>لا يمكن حذف الفرع لوجود مستخدمين نشطين مرتبطين به.</summary>
    public const string BranchHasUsers = "BRANCH_HAS_ACTIVE_USERS";

    // ── Catalog (دليل المنتجات والتصنيفات) ──────────────────
    /// <summary>التصنيف المحدد غير موجود.</summary>
    public const string CategoryNotFound = "CATEGORY_NOT_FOUND";

    /// <summary>لا يمكن حذف التصنيف لاحتوائه على منتجات تابعة.</summary>
    public const string CategoryHasProducts = "CATEGORY_HAS_PRODUCTS";

    /// <summary>مرجع دائري في شجرة التصنيفات (التصنيف الأب لا يمكن أن يكون التصنيف نفسه أو أحد أبنائه).</summary>
    public const string CategoryCircularRef = "CATEGORY_CIRCULAR_REFERENCE";

    /// <summary>اسم التصنيف مستخدم مسبقاً ضمن نفس المستوى أو المستأجر.</summary>
    public const string CategoryNameExists = "CATEGORY_NAME_EXISTS";

    /// <summary>المنتج المحدد غير موجود.</summary>
    public const string ProductNotFound = "PRODUCT_NOT_FOUND";

    /// <summary>المنتج مستخدم في حركات وفواتير سابقة ولا يمكن حذفه.</summary>
    public const string ProductInUse = "PRODUCT_IN_USE";

    /// <summary>وحدة القياس المحددة غير موجودة.</summary>
    public const string UnitNotFound = "UNIT_NOT_FOUND";

    /// <summary>اسم وحدة القياس مستخدم مسبقاً.</summary>
    public const string UnitNameExists = "UNIT_NAME_EXISTS";

    /// <summary>وحدة القياس مستخدمة في منتجات حالية ولا يمكن حذفها.</summary>
    public const string UnitInUse = "UNIT_IN_USE";

    /// <summary>رابط وحدة قياس المنتج غير موجود.</summary>
    public const string ProductUnitNotFound = "PRODUCT_UNIT_NOT_FOUND";

    /// <summary>وحدة القياس مكررة لهذا المنتج.</summary>
    public const string ProductUnitDuplicate = "PRODUCT_UNIT_DUPLICATE";

    /// <summary>الباركود غير موجود.</summary>
    public const string BarCodeNotFound = "BARCODE_NOT_FOUND";

    /// <summary>الباركود مسجل مسبقاً لمنتج أو وحدة أخرى.</summary>
    public const string BarCodeDuplicate = "BARCODE_DUPLICATE";

    /// <summary>الباركود مستخدم في عمليات تشغيلية.</summary>
    public const string BarCodeInUse = "BARCODE_IN_USE";

    /// <summary>الصورة المحددة للمنتج غير موجودة.</summary>
    public const string ImageNotFound = "IMAGE_NOT_FOUND";

    // ── File Upload (رفع الملفات) ──────────────────────────
    /// <summary>نوع الملف غير مدعوم أو غير مسموح برفعه.</summary>
    public const string InvalidFileType = "INVALID_FILE_TYPE";

    /// <summary>حجم الملف يتجاوز الحد الأقصى المسموح به.</summary>
    public const string FileTooLarge = "FILE_TOO_LARGE";

    /// <summary>فشلت عملية رفع وتخزين الملف على الخادم.</summary>
    public const string UploadFailed = "UPLOAD_FAILED";

    // ── Suppliers (الموردين) ───────────────────────────────
    /// <summary>المورد المحدد غير موجود.</summary>
    public const string SupplierNotFound = "SUPPLIER_NOT_FOUND";

    /// <summary>اسم المورد مسجل مسبقاً لدى نفس المستأجر.</summary>
    public const string SupplierNameExists = "SUPPLIER_NAME_EXISTS";

    // ── Warehouses (المستودعات) ────────────────────────────
    /// <summary>المستودع المحدد غير موجود.</summary>
    public const string WarehouseNotFound = "WAREHOUSE_NOT_FOUND";

    /// <summary>اسم المستودع مستخدم مسبقاً لنفس الفرع أو المستأجر.</summary>
    public const string WarehouseNameExists = "WAREHOUSE_NAME_EXISTS";

    /// <summary>المستودع يحتوي على رصيد مخزني ولا يمكن حذفه مباشرة.</summary>
    public const string WarehouseHasStock = "WAREHOUSE_HAS_STOCK";

    // ── Stock (المخزون) ────────────────────────────────────
    /// <summary>سجل المخزون المحدد غير موجود.</summary>
    public const string StockNotFound = "STOCK_NOT_FOUND";

    /// <summary>الكمية المتوفرة في المخزون غير كافية لإتمام الحركة.</summary>
    public const string InsufficientStock = "INSUFFICIENT_STOCK";

    // ── Purchase Orders (أوامر الشراء) ─────────────────────
    /// <summary>أمر الشراء المحدد غير موجود.</summary>
    public const string PurchaseOrderNotFound = "PURCHASE_ORDER_NOT_FOUND";

    /// <summary>رقم أمر الشراء مستخدم ومسجل مسبقاً.</summary>
    public const string PurchaseOrderNumberExists = "PURCHASE_ORDER_NUMBER_EXISTS";

    /// <summary>حالة أمر الشراء لا تسمح بإجراء التعديل أو العملية المطلوبة.</summary>
    public const string PurchaseOrderInvalidStatus = "PURCHASE_ORDER_INVALID_STATUS";

    // ── Customers (العملاء) ────────────────────────────────
    /// <summary>العميل المحدد غير موجود.</summary>
    public const string CustomerNotFound = "CUSTOMER_NOT_FOUND";

    /// <summary>كود العميل مستخدم ومكرر.</summary>
    public const string CustomerCodeExists = "CUSTOMER_CODE_EXISTS";

    /// <summary>تم تجاوز الحد الائتماني المسموح به للعميل.</summary>
    public const string CustomerCreditExceeded = "CUSTOMER_CREDIT_EXCEEDED";

    // ── Sales Invoices (فواتير المبيعات) ───────────────────
    /// <summary>فاتورة المبيعات المحددة غير موجودة.</summary>
    public const string SalesInvoiceNotFound = "SALES_INVOICE_NOT_FOUND";

    /// <summary>رقم فاتورة المبيعات مسجل مسبقاً.</summary>
    public const string SalesInvoiceNumberExists = "SALES_INVOICE_NUMBER_EXISTS";

    /// <summary>حالة فاتورة المبيعات لا تسمح بتنفيذ الإجراء المطلوب.</summary>
    public const string SalesInvoiceInvalidStatus = "SALES_INVOICE_INVALID_STATUS";

    /// <summary>الرصيد في صالة العرض أو المستودع المباشر غير كافٍ لإتمام الفاتورة.</summary>
    public const string InsufficientShowroomStock = "INSUFFICIENT_SHOWROOM_STOCK";

    // ── Sales Returns (مرتجعات المبيعات) ───────────────────
    /// <summary>سند مرتجع المبيعات غير موجود.</summary>
    public const string SalesReturnNotFound = "SALES_RETURN_NOT_FOUND";

    /// <summary>رقم سند مرتجع المبيعات مستخدم مسبقاً.</summary>
    public const string SalesReturnNumberExists = "SALES_RETURN_NUMBER_EXISTS";

    /// <summary>الكمية المرتجعة تتجاوز الكمية المباعة الأصلية في الفاتورة.</summary>
    public const string SalesReturnExceedsSold = "SALES_RETURN_EXCEEDS_SOLD";

    // ── Purchase Returns (مرتجعات المشتريات) ────────────────
    /// <summary>سند مرتجع المشتريات غير موجود.</summary>
    public const string PurchaseReturnNotFound = "PURCHASE_RETURN_NOT_FOUND";

    /// <summary>رقم سند مرتجع المشتريات مستخدم مسبقاً.</summary>
    public const string PurchaseReturnNumberExists = "PURCHASE_RETURN_NUMBER_EXISTS";

    /// <summary>المستودع المحدد لمرتجع المشتريات غير صالح أو غير مرتبط بالفاتورة الأصلية.</summary>
    public const string PurchaseReturnInvalidWarehouse = "PURCHASE_RETURN_INVALID_WAREHOUSE";

    /// <summary>كمية مرتجع المشتريات غير صالحة أو تتجاوز رصيد الفاتورة.</summary>
    public const string PurchaseReturnInvalidQuantity = "PURCHASE_RETURN_INVALID_QUANTITY";

    // ── Stock Transfers (التحويلات المخزنية) ────────────────
    /// <summary>سند تحويل المخزون غير موجود.</summary>
    public const string StockTransferNotFound = "STOCK_TRANSFER_NOT_FOUND";

    /// <summary>رقم سند التحويل المخزني مسجل مسبقاً.</summary>
    public const string StockTransferNumberExists = "STOCK_TRANSFER_NUMBER_EXISTS";

    /// <summary>حالة سند التحويل المخزني لا تسمح بهذا الإجراء.</summary>
    public const string StockTransferInvalidStatus = "STOCK_TRANSFER_INVALID_STATUS";

    /// <summary>لا يمكن التحويل من وإلى نفس المستودع.</summary>
    public const string StockTransferSameWarehouse = "STOCK_TRANSFER_SAME_WAREHOUSE";

    // ── Stock Adjustments (تسويات المخزون) ──────────────────
    /// <summary>سجل تسوية المخزون غير موجود.</summary>
    public const string StockAdjustmentNotFound = "STOCK_ADJUSTMENT_NOT_FOUND";

    /// <summary>رقم سند تسوية المخزون مسجل مسبقاً.</summary>
    public const string StockAdjustmentNumberExists = "STOCK_ADJUSTMENT_NUMBER_EXISTS";
}
