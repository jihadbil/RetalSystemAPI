using System;
using System.Threading;
using System.Threading.Tasks;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Interfaces;

/// <summary>
/// واجهة نمط وحدة العمل (Unit of Work Pattern) لتنسيق وحفظ التعديلات كـ Transaction واحدة مؤمنة عبر جميع المستودعات.
/// </summary>
public interface IUnitOfWork : IAsyncDisposable
{
    // ── Repositories ──────────────────────────────────────────

    /// <summary>مستودع بيانات المستأجرين (Tenants)</summary>
    IRepository<Tenant> Tenants { get; }

    /// <summary>مستودع بيانات الفروع (Branches)</summary>
    IRepository<Branch> Branches { get; }

    /// <summary>مستودع بيانات هواتف الفروع (Branch Phones)</summary>
    IRepository<BranchPhone> BranchPhones { get; }

    /// <summary>مستودع بيانات التصنيفات (Categories)</summary>
    IRepository<Category> Categories { get; }

    /// <summary>مستودع بيانات الأصناف والمنتجات (Products)</summary>
    IRepository<Product> Products { get; }

    /// <summary>مستودع بيانات وحدات قياس وتعبئة الأصناف (Product Units)</summary>
    IRepository<ProductUnit> ProductUnits { get; }

    /// <summary>مستودع بيانات باركودات ونكهات الأصناف (Product Barcodes)</summary>
    IRepository<ProductBarCode> ProductBarCodes { get; }

    /// <summary>مستودع بيانات صور الأصناف (Product Images)</summary>
    IRepository<ProductImage> ProductImages { get; }

    /// <summary>مستودع بيانات الوحدات الأساسية (Units)</summary>
    IRepository<Unit> Units { get; }

    /// <summary>مستودع بيانات الموردين (Suppliers)</summary>
    IRepository<Supplier> Suppliers { get; }

    /// <summary>مستودع بيانات هواتف الموردين (Supplier Phones)</summary>
    IRepository<SupplierPhone> SupplierPhones { get; }

    /// <summary>مستودع بيانات المخازن والمستودعات والصالات (Warehouses)</summary>
    IRepository<Warehouse> Warehouses { get; }

    /// <summary>مستودع بيانات أرصدة المستودعات الرئيسية لكل نكهة وباركود (Storage Stocks)</summary>
    IRepository<StorgeStock> StorgeStocks { get; }

    /// <summary>مستودع بيانات أرصدة صالات العرض لكل صنف (Showroom Stocks)</summary>
    IRepository<ShowroomStock> ShowroomStocks { get; }

    /// <summary>مستودع بيانات أوامر التحويل المخزني (Stock Transfers)</summary>
    IRepository<StockTransfer> StockTransfers { get; }

    /// <summary>مستودع بيانات بنود التحويل المخزني (Stock Transfer Items)</summary>
    IRepository<StockTransferItem> StockTransferItems { get; }

    /// <summary>مستودع بيانات تسويات الجرد المخزني (Stock Adjustments)</summary>
    IRepository<StockAdjustment> StockAdjustments { get; }

    /// <summary>مستودع بيانات بنود تسويات الجرد المخزني (Stock Adjustment Items)</summary>
    IRepository<StockAdjustmentItem> StockAdjustmentItems { get; }

    /// <summary>مستودع بيانات طلبات الشراء (Purchase Orders)</summary>
    IRepository<PurchaseOrder> PurchaseOrders { get; }

    /// <summary>مستودع بيانات بنود طلبات الشراء (Purchase Order Items)</summary>
    IRepository<PurchaseOrderItem> PurchaseOrderItems { get; }

    /// <summary>مستودع بيانات فواتير المشتريات (Purchase Invoices)</summary>
    IRepository<PurchaseInvoice> PurchaseInvoices { get; }

    /// <summary>مستودع بيانات بنود فواتير المشتريات المجمعة (Purchase Invoice Items)</summary>
    IRepository<PurchaseInvoiceItem> PurchaseInvoiceItems { get; }

    /// <summary>مستودع بيانات تفصيل النكهات والباركودات المستلمة في فواتير المشتريات (Purchase Invoice Item Breakdowns)</summary>
    IRepository<PurchaseInvoiceItemBreakdown> PurchaseInvoiceItemBreakdowns { get; }

    /// <summary>مستودع بيانات فواتير مرتجع المشتريات (Purchase Returns)</summary>
    IRepository<PurchaseReturn> PurchaseReturns { get; }

    /// <summary>مستودع بيانات بنود فواتير مرتجع المشتريات (Purchase Return Items)</summary>
    IRepository<PurchaseReturnItem> PurchaseReturnItems { get; }

    /// <summary>مستودع بيانات الزبائن والعملاء (Customers)</summary>
    IRepository<Customer> Customers { get; }

    /// <summary>مستودع بيانات هواتف العملاء (Customer Phones)</summary>
    IRepository<CustomerPhone> CustomerPhones { get; }

    /// <summary>مستودع بيانات فواتير المبيعات (Sales Invoices)</summary>
    IRepository<SalesInvoice> SalesInvoices { get; }

    /// <summary>مستودع بيانات بنود فواتير المبيعات (Sales Invoice Items)</summary>
    IRepository<SalesInvoiceItem> SalesInvoiceItems { get; }

    /// <summary>مستودع بيانات فواتير مرتجع المبيعات (Sales Returns)</summary>
    IRepository<SalesReturn> SalesReturns { get; }

    /// <summary>مستودع بيانات بنود فواتير مرتجع المبيعات (Sales Return Items)</summary>
    IRepository<SalesReturnItem> SalesReturnItems { get; }

    // ── Transaction Control ───────────────────────────────────

    /// <summary>
    /// حفظ جميع التغييرات المعلقة في سياق قاعدة البيانات بصورة غير متزامنة.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    /// <returns>عدد السجلات المتأثرة بعملية الحفظ</returns>
    Task<int> SaveChangesAsync(CancellationToken ct = default);

    /// <summary>
    /// جلب قائمة بالكيانات المتتبعة حالياً في ChangeTracker الخاص بـ DbContext.
    /// </summary>
    /// <returns>مجموعة مدخلات الكيانات المتتبعة</returns>
    IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> ChangeTrackerEntries();

    /// <summary>
    /// بدء معاملة ذرية صريحة (Database Transaction) لضمان تنفيذ عدة عمليات كوحدة واحدة.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// تثبيت وتأكيد التغييرات المنفذة داخل المعاملة الحالية في قاعدة البيانات.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    Task CommitTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// التراجع عن جميع التغييرات المنفذة داخل المعاملة الحالية في حال حدوث أي خطأ.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية</param>
    Task RollbackTransactionAsync(CancellationToken ct = default);
}
