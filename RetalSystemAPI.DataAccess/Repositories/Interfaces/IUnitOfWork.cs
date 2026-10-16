using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;
/// <summary>
/// واجهة نمط وحدة العمل (Unit of Work Pattern) لتنسيق وحفظ التعديلات كـ Transaction واحدة مؤمنة عبر جميع المستودعات المتخصصة.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    // ── Specialized Repositories ──────────────────────────────
    /// <summary>مستودع بيانات المستأجرين (Tenants)</summary>
    ITenantRepository Tenants { get; }

    /// <summary>مستودع بيانات الفروع وهواتفها (Branches)</summary>
    IBranchRepository Branches { get; }

    /// <summary>مستودع بيانات التصنيفات (Categories)</summary>
    ICategoryRepository Categories { get; }

    /// <summary>مستودع بيانات الوحدات الأساسية (Units)</summary>
    IUnitRepository Units { get; }

    /// <summary>مستودع بيانات الأصناف والمنتجات ووحداتها وباركوداتها وصورها (Products)</summary>
    IProductRepository Products { get; }

    /// <summary>مستودع بيانات الموردين وهواتفهم وحركات حساباتهم (Suppliers)</summary>
    ISupplierRepository Suppliers { get; }

    /// <summary>مستودع بيانات الزبائن والعملاء وهواتفهم وحركات حساباتهم (Customers)</summary>
    ICustomerRepository Customers { get; }

    /// <summary>مستودع بيانات المخازن والمستودعات والصالات (Warehouses)</summary>
    IWarehouseRepository Warehouses { get; }

    /// <summary>مستودع بيانات أرصدة المستودعات الرئيسية وصالات العرض (Stock)</summary>
    IStockRepository Stock { get; }

    /// <summary>مستودع بيانات أوامر التحويل المخزني وبنودها (Stock Transfers)</summary>
    IStockTransferRepository StockTransfers { get; }

    /// <summary>مستودع بيانات تسويات الجرد المخزني وبنودها (Stock Adjustments)</summary>
    IStockAdjustmentRepository StockAdjustments { get; }

    /// <summary>مستودع بيانات فواتير المبيعات وبنودها (Sales Invoices)</summary>
    ISalesInvoiceRepository SalesInvoices { get; }

    /// <summary>مستودع بيانات فواتير مرتجع المبيعات وبنودها (Sales Returns)</summary>
    ISalesReturnRepository SalesReturns { get; }

    /// <summary>مستودع بيانات طلبات الشراء وبنودها (Purchase Orders)</summary>
    IPurchaseOrderRepository PurchaseOrders { get; }

    /// <summary>مستودع بيانات فواتير المشتريات وبنودها وتفاصيل النكهات (Purchase Invoices)</summary>
    IPurchaseInvoiceRepository PurchaseInvoices { get; }

    /// <summary>مستودع بيانات فواتير مرتجع المشتريات وبنودها (Purchase Returns)</summary>
    IPurchaseReturnRepository PurchaseReturns { get; }

    /// <summary>مستودع بيانات الحسابات المالية وإدارة الخزائن والمصروفات (Accounting)</summary>
    IAccountingRepository Accounting { get; }

    // ── Transaction Control ───────────────────────────────────
    /// <summary>
    /// حفظ جميع التغييرات المعلقة في سياق قاعدة البيانات بصورة غير متزامنة.
    /// </summary>
    /// <param name = "ct">رمز إلغاء العملية</param>
    /// <returns>عدد السجلات المتأثرة بعملية الحفظ</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    /// <summary>
    /// جلب قائمة بالكيانات المتتبعة حالياً في ChangeTracker الخاص بـ DbContext.
    /// </summary>
    /// <returns>مجموعة مدخلات الكيانات المتتبعة</returns>
    IEnumerable<EntityEntry> ChangeTrackerEntries();
    /// <summary>
    /// بدء معاملة ذرية صريحة (Database Transaction) لضمان تنفيذ عدة عمليات كوحدة واحدة.
    /// </summary>
    /// <param name = "ct">رمز إلغاء العملية</param>
    Task BeginTransactionAsync(CancellationToken ct = default);
    /// <summary>
    /// تثبيت وتأكيد التغييرات المنفذة داخل المعاملة الحالية في قاعدة البيانات.
    /// </summary>
    /// <param name = "ct">رمز إلغاء العملية</param>
    Task CommitTransactionAsync(CancellationToken ct = default);
    /// <summary>
    /// التراجع عن جميع التغييرات المنفذة داخل المعاملة الحالية في حال حدوث أي خطأ.
    /// يُلغى التتبع ويُمنع الحفظ وبدء المعاملات بهذا السياق بعد التراجع؛ يجب إنشاء نطاق جديد.
    /// </summary>
    /// <param name = "ct">رمز إلغاء العملية</param>
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
