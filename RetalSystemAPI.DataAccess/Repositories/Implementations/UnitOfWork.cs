using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// التنفيذ الأحدث لنمط وحدة العمل (Unit of Work) الذي يدير المعاملات الموحدة والمستودعات المتخصصة.
/// يوفر نقطة وصول موحدة لكافة المستودعات وينظم دورة حياة المعاملات الذرية وضمان عزلها وإلغائها الآمن.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    // الحقول الخلفية للتهيئة الكسولة (Lazy Loading) للمستودعات المتخصصة
    private ITenantRepository? _tenants;
    private IBranchRepository? _branches;
    private ICategoryRepository? _categories;
    private IUnitRepository? _units;
    private IProductRepository? _products;
    private ISupplierRepository? _suppliers;
    private ICustomerRepository? _customers;
    private IWarehouseRepository? _warehouses;
    private IStockRepository? _stock;
    private IStockTransferRepository? _stockTransfers;
    private IStockAdjustmentRepository? _stockAdjustments;
    private ISalesInvoiceRepository? _salesInvoices;
    private ISalesReturnRepository? _salesReturns;
    private IPurchaseOrderRepository? _purchaseOrders;
    private IPurchaseInvoiceRepository? _purchaseInvoices;
    private IPurchaseReturnRepository? _purchaseReturns;
    private IAccountingRepository? _accounting;

    /// <summary>
    /// بناء كائن وحدة العمل وربطه بسياق قاعدة البيانات AppDbContext.
    /// </summary>
    /// <param name="context">سياق قاعدة البيانات الحالي.</param>
    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc />
    public ITenantRepository Tenants => _tenants ??= new TenantRepository(_context);

    /// <inheritdoc />
    public IBranchRepository Branches => _branches ??= new BranchRepository(_context);

    /// <inheritdoc />
    public ICategoryRepository Categories => _categories ??= new CategoryRepository(_context);

    /// <inheritdoc />
    public IUnitRepository Units => _units ??= new UnitRepository(_context);

    /// <inheritdoc />
    public IProductRepository Products => _products ??= new ProductRepository(_context);

    /// <inheritdoc />
    public ISupplierRepository Suppliers => _suppliers ??= new SupplierRepository(_context);

    /// <inheritdoc />
    public ICustomerRepository Customers => _customers ??= new CustomerRepository(_context);

    /// <inheritdoc />
    public IWarehouseRepository Warehouses => _warehouses ??= new WarehouseRepository(_context);

    /// <inheritdoc />
    public IStockRepository Stock => _stock ??= new StockRepository(_context);

    /// <inheritdoc />
    public IStockTransferRepository StockTransfers => _stockTransfers ??= new StockTransferRepository(_context);

    /// <inheritdoc />
    public IStockAdjustmentRepository StockAdjustments => _stockAdjustments ??= new StockAdjustmentRepository(_context);

    /// <inheritdoc />
    public ISalesInvoiceRepository SalesInvoices => _salesInvoices ??= new SalesInvoiceRepository(_context);

    /// <inheritdoc />
    public ISalesReturnRepository SalesReturns => _salesReturns ??= new SalesReturnRepository(_context);

    /// <inheritdoc />
    public IPurchaseOrderRepository PurchaseOrders => _purchaseOrders ??= new PurchaseOrderRepository(_context);

    /// <inheritdoc />
    public IPurchaseInvoiceRepository PurchaseInvoices => _purchaseInvoices ??= new PurchaseInvoiceRepository(_context);

    /// <inheritdoc />
    public IPurchaseReturnRepository PurchaseReturns => _purchaseReturns ??= new PurchaseReturnRepository(_context);

    /// <inheritdoc />
    public IAccountingRepository Accounting => _accounting ??= new AccountingRepository(_context);

    // ── Transaction Control ───────────────────────────────────

    /// <inheritdoc />
    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // حفظ جميع التغييرات المتتبعة في الذاكرة وكتابتها في قاعدة البيانات
        return await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public IEnumerable<EntityEntry> ChangeTrackerEntries()
    {
        // استرجاع كافة مدخلات الكيانات المسجلة في متعقب التغييرات
        return _context.ChangeTracker.Entries();
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        // التحقق من صلاحية السياق وعدم إلغائه مسبقاً
        _context.EnsureTransactionUsable();

        // التأكد من عدم وجود معاملة نشطة بالفعل على نفس نطاق وحدة العمل
        if (_transaction is not null)
        {
            throw new InvalidOperationException("توجد معاملة مفعلة بالفعل على وحدة العمل الحالية.");
        }

        // بدء معاملة صريحة جديدة على مستوى قاعدة البيانات
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        // الاحتفاظ بمرجع المعاملة الحالية
        var transaction = _transaction;
        if (transaction is null)
        {
            throw new InvalidOperationException("لا توجد معاملة مفعلة حالياً ليتم اعتمادها (Commit).");
        }

        try
        {
            // ترحيل كافة التغييرات المعلقة في الذاكرة أولاً قبل تثبيت المعاملة
            await _context.SaveChangesAsync(ct);

            // تثبيت واعتماد المعاملة نهائياً في قاعدة البيانات
            await transaction.CommitAsync(ct);
        }
        catch
        {
            try
            {
                // محاولة التراجع عن المعاملة حتى لو تم إلغاء رمز الطلب CancellationToken
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch
            {
                // تجاهل أخطاء التراجع لضمان إبراز الخطأ الأصلي المسبب لفشل الحفظ
            }
            finally
            {
                // إبطال السياق لمنع استخدامه بعد فشل المعاملة وضمان عدم تسرب بيانات فاسدة
                _context.InvalidateAfterRollback();
            }

            // إعادة رمي الخطأ الأصلي للمعالجة في الطبقات العليا
            throw;
        }
        finally
        {
            try
            {
                // التخلص من موارد المعاملة
                await transaction.DisposeAsync();
            }
            finally
            {
                // تصفير مرجع المعاملة في حال كان يطابق المعاملة المنتهية
                if (ReferenceEquals(_transaction, transaction)) _transaction = null;
            }
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        // التحقق من وجود معاملة قائمة للتراجع عنها
        var transaction = _transaction;
        if (transaction is null) return;

        try
        {
            // التراجع الصريح عن كافة التغييرات باستخدام رمز غير قابل للإلغاء
            await transaction.RollbackAsync(CancellationToken.None);
        }
        finally
        {
            // إبطال صلاحية السياق لحماية حالة التطبيق من التعارضات
            _context.InvalidateAfterRollback();
            try
            {
                // التخلص من كائن المعاملة
                await transaction.DisposeAsync();
            }
            finally
            {
                // تصفير مرجع المعاملة
                if (ReferenceEquals(_transaction, transaction)) _transaction = null;
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        // التخلص من المعاملة المفتوحة إن وجدت لضمان تحرير القفل وقنوات الاتصال
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        // التخلص من سياق قاعدة البيانات وتحرير اتصاله
        await _context.DisposeAsync();

        // إيقاف استدعاء Finalizer لتوفير موارد جامع القمامة
        GC.SuppressFinalize(this);
    }
}
