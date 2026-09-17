using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// التنفيذ الأحدث لنمط وحدة العمل (Unit of Work) الذي يدير المعاملات الموحدة والمستودعات العامة لـ EF Core.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    // Repositories Backing Fields
    private IRepository<Tenant>? _tenants;
    private IRepository<Branch>? _branches;
    private IRepository<BranchPhone>? _branchPhones;
    private IRepository<Category>? _categories;
    private IRepository<Product>? _products;
    private IRepository<ProductUnit>? _productUnits;
    private IRepository<ProductBarCode>? _productBarCodes;
    private IRepository<ProductImage>? _productImages;
    private IRepository<Unit>? _units;
    private IRepository<Supplier>? _suppliers;
    private IRepository<SupplierPhone>? _supplierPhones;
    private IRepository<Warehouse>? _warehouses;
    private IRepository<StorgeStock>? _storgeStocks;
    private IRepository<ShowroomStock>? _showroomStocks;
    private IRepository<StockTransfer>? _stockTransfers;
    private IRepository<StockTransferItem>? _stockTransferItems;
    private IRepository<StockAdjustment>? _stockAdjustments;
    private IRepository<StockAdjustmentItem>? _stockAdjustmentItems;
    private IRepository<PurchaseOrder>? _purchaseOrders;
    private IRepository<PurchaseOrderItem>? _purchaseOrderItems;
    private IRepository<PurchaseInvoice>? _purchaseInvoices;
    private IRepository<PurchaseInvoiceItem>? _purchaseInvoiceItems;
    private IRepository<PurchaseInvoiceItemBreakdown>? _purchaseInvoiceItemBreakdowns;
    private IRepository<PurchaseReturn>? _purchaseReturns;
    private IRepository<PurchaseReturnItem>? _purchaseReturnItems;
    private IRepository<Customer>? _customers;
    private IRepository<CustomerPhone>? _customerPhones;
    private IRepository<SalesInvoice>? _salesInvoices;
    private IRepository<SalesInvoiceItem>? _salesInvoiceItems;
    private IRepository<SalesReturn>? _salesReturns;
    private IRepository<SalesReturnItem>? _salesReturnItems;

    /// <summary>
    /// بناء كائن وحدة العمل وربطه بسياق قاعدة البيانات AppDbContext.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات</param>
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public IRepository<Tenant> Tenants => _tenants ??= new Repository<Tenant>(_context);

    /// <inheritdoc />
    public IRepository<Branch> Branches => _branches ??= new Repository<Branch>(_context);

    /// <inheritdoc />
    public IRepository<BranchPhone> BranchPhones => _branchPhones ??= new Repository<BranchPhone>(_context);

    /// <inheritdoc />
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);

    /// <inheritdoc />
    public IRepository<Product> Products => _products ??= new Repository<Product>(_context);

    /// <inheritdoc />
    public IRepository<ProductUnit> ProductUnits => _productUnits ??= new Repository<ProductUnit>(_context);

    /// <inheritdoc />
    public IRepository<ProductBarCode> ProductBarCodes => _productBarCodes ??= new Repository<ProductBarCode>(_context);

    /// <inheritdoc />
    public IRepository<ProductImage> ProductImages => _productImages ??= new Repository<ProductImage>(_context);

    /// <inheritdoc />
    public IRepository<Unit> Units => _units ??= new Repository<Unit>(_context);

    /// <inheritdoc />
    public IRepository<Supplier> Suppliers => _suppliers ??= new Repository<Supplier>(_context);

    /// <inheritdoc />
    public IRepository<SupplierPhone> SupplierPhones => _supplierPhones ??= new Repository<SupplierPhone>(_context);

    /// <inheritdoc />
    public IRepository<Warehouse> Warehouses => _warehouses ??= new Repository<Warehouse>(_context);

    /// <inheritdoc />
    public IRepository<StorgeStock> StorgeStocks => _storgeStocks ??= new Repository<StorgeStock>(_context);

    /// <inheritdoc />
    public IRepository<ShowroomStock> ShowroomStocks => _showroomStocks ??= new Repository<ShowroomStock>(_context);

    /// <inheritdoc />
    public IRepository<StockTransfer> StockTransfers => _stockTransfers ??= new Repository<StockTransfer>(_context);

    /// <inheritdoc />
    public IRepository<StockTransferItem> StockTransferItems => _stockTransferItems ??= new Repository<StockTransferItem>(_context);

    /// <inheritdoc />
    public IRepository<StockAdjustment> StockAdjustments => _stockAdjustments ??= new Repository<StockAdjustment>(_context);

    /// <inheritdoc />
    public IRepository<StockAdjustmentItem> StockAdjustmentItems => _stockAdjustmentItems ??= new Repository<StockAdjustmentItem>(_context);

    /// <inheritdoc />
    public IRepository<PurchaseOrder> PurchaseOrders => _purchaseOrders ??= new Repository<PurchaseOrder>(_context);

    /// <inheritdoc />
    public IRepository<PurchaseOrderItem> PurchaseOrderItems => _purchaseOrderItems ??= new Repository<PurchaseOrderItem>(_context);

    /// <inheritdoc />
    public IRepository<PurchaseInvoice> PurchaseInvoices => _purchaseInvoices ??= new Repository<PurchaseInvoice>(_context);

    /// <inheritdoc />
    public IRepository<PurchaseInvoiceItem> PurchaseInvoiceItems => _purchaseInvoiceItems ??= new Repository<PurchaseInvoiceItem>(_context);

    /// <inheritdoc />
    public IRepository<PurchaseInvoiceItemBreakdown> PurchaseInvoiceItemBreakdowns => _purchaseInvoiceItemBreakdowns ??= new Repository<PurchaseInvoiceItemBreakdown>(_context);

    /// <inheritdoc />
    public IRepository<PurchaseReturn> PurchaseReturns => _purchaseReturns ??= new Repository<PurchaseReturn>(_context);

    /// <inheritdoc />
    public IRepository<PurchaseReturnItem> PurchaseReturnItems => _purchaseReturnItems ??= new Repository<PurchaseReturnItem>(_context);

    /// <inheritdoc />
    public IRepository<Customer> Customers => _customers ??= new Repository<Customer>(_context);

    /// <inheritdoc />
    public IRepository<CustomerPhone> CustomerPhones => _customerPhones ??= new Repository<CustomerPhone>(_context);

    /// <inheritdoc />
    public IRepository<SalesInvoice> SalesInvoices => _salesInvoices ??= new Repository<SalesInvoice>(_context);

    /// <inheritdoc />
    public IRepository<SalesInvoiceItem> SalesInvoiceItems => _salesInvoiceItems ??= new Repository<SalesInvoiceItem>(_context);

    /// <inheritdoc />
    public IRepository<SalesReturn> SalesReturns => _salesReturns ??= new Repository<SalesReturn>(_context);

    /// <inheritdoc />
    public IRepository<SalesReturnItem> SalesReturnItems => _salesReturnItems ??= new Repository<SalesReturnItem>(_context);

    /// <summary>
    /// حفظ جميع التغييرات المعلقة في سياق قاعدة البيانات بصورة غير متزامنة.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>عدد السجلات المتأثرة بعملية الحفظ</returns>
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // تمرير أمر حفظ التغييرات لسياق قاعدة البيانات AppDbContext
        return await _context.SaveChangesAsync(ct);
    }

    /// <summary>
    /// جلب مدخلات الكيانات المتتبعة في ChangeTracker لمراقبة حالات الكيانات.
    /// </summary>
    /// <returns>قائمة مدخلات الكيانات المتتبعة</returns>
    public System.Collections.Generic.IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> ChangeTrackerEntries()
    {
        // استرجاع كافة مدخلات الكيانات الخاضعة للتتبع حالياً في الذاكرة
        return _context.ChangeTracker.Entries();
    }

    /// <summary>
    /// بدء معاملة ذرية صريحة (Database Transaction) لتنفيذ عدة عمليات كوحدة لا تتجزأ.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        // فتح معاملة جديدة على مستوى محرك قاعدة البيانات وتخزين مرجعها
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    /// <summary>
    /// اعتماد وتثبيت التغييرات داخل المعاملة الحالية في قاعدة البيانات مع التراجع التلقائي عند الخطأ.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        // التحقق من وجود معاملة نشطة قبل محاولة الاعتماد
        if (_transaction is null)
        {
            // إطلاق استثناء في حال عدم وجود معاملة مفتوحة
            throw new InvalidOperationException("لا توجد معاملة مفعلة حالياً ليتم اعتمادها (Commit).");
        }

        try
        {
            // حفظ كافة التغييرات المعلقة في السياق أولاً داخل نطاق المعاملة
            await _context.SaveChangesAsync(ct);
            // تثبيت واعتماد المعاملة نهائياً في قاعدة البيانات
            await _transaction.CommitAsync(ct);
        }
        catch
        {
            // التراجع عن أي تعديلات طرأت أثناء المعاملة عند حدوث أي استثناء
            await RollbackTransactionAsync(ct);
            // إعادة إطلاق الاستثناء لمعالجته في الطبقات الأعلى
            throw;
        }
        finally
        {
            // تحرير موارد كائن المعاملة
            await _transaction.DisposeAsync();
            // تصفير مرجع المعاملة للإشارة إلى انتهائها
            _transaction = null;
        }
    }

    /// <summary>
    /// التراجع عن التغييرات المنفذة داخل المعاملة الحالية وإلغاء تأثيرها.
    /// </summary>
    /// <param name="ct">رمز إلغاء العملية غير المتزامنة</param>
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        // إذا لم تكن هناك معاملة نشطة يتم الخروج فوراً
        if (_transaction is null) return;

        try
        {
            // تنفيذ أمر التراجع Rollback على المعاملة
            await _transaction.RollbackAsync(ct);
        }
        finally
        {
            // تحرير موارد المعاملة
            await _transaction.DisposeAsync();
            // تصفير مرجع المعاملة
            _transaction = null;
        }
    }

    /// <summary>
    /// تحرير موارد المعاملة وسياق قاعدة البيانات بشكل غير متزامن لمنع تسريب الاتصالات.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        // التحقق من وجود أي معاملة متبقية لم يتم إغلاقها
        if (_transaction is not null)
        {
            // تحرير موارد المعاملة المعلقة
            await _transaction.DisposeAsync();
            // تصفير المرجع
            _transaction = null;
        }

        // تحرير موارد سياق قاعدة البيانات AppDbContext
        await _context.DisposeAsync();
        // إبلاغ جامع المهملات بعدم الحاجة لاستدعاء Finalizer
        GC.SuppressFinalize(this);
    }
}
