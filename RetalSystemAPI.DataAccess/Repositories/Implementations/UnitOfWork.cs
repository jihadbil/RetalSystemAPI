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
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;

    // Specialized Repositories Backing Fields
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
    /// <param name="context">سياق قاعدة البيانات</param>
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
        return await _context.SaveChangesAsync(ct);
    }

    /// <inheritdoc />
    public IEnumerable<EntityEntry> ChangeTrackerEntries()
    {
        return _context.ChangeTracker.Entries();
    }

    /// <inheritdoc />
    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _context.EnsureTransactionUsable();
        if (_transaction is not null)
        {
            throw new InvalidOperationException("توجد معاملة مفعلة بالفعل على وحدة العمل الحالية.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    /// <inheritdoc />
    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        var transaction = _transaction;
        if (transaction is null)
        {
            throw new InvalidOperationException("لا توجد معاملة مفعلة حالياً ليتم اعتمادها (Commit).");
        }

        try
        {
            // Keep the historical UnitOfWork contract: callers may stage tracked
            // changes and rely on Commit to flush them before committing.
            await _context.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            try
            {
                // Preserve the commit exception even when the request token was
                // cancelled or rollback itself cannot contact the database.
                await transaction.RollbackAsync(CancellationToken.None);
            }
            catch
            {
                // Disposing the transaction below is the final rollback safeguard.
            }
            finally
            {
                _context.InvalidateAfterRollback();
            }

            throw;
        }
        finally
        {
            try
            {
                await transaction.DisposeAsync();
            }
            finally
            {
                if (ReferenceEquals(_transaction, transaction)) _transaction = null;
            }
        }
    }

    /// <inheritdoc />
    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        var transaction = _transaction;
        if (transaction is null) return;

        try
        {
            // Cleanup must still run when the request has been cancelled.
            await transaction.RollbackAsync(CancellationToken.None);
        }
        finally
        {
            _context.InvalidateAfterRollback();
            try
            {
                await transaction.DisposeAsync();
            }
            finally
            {
                if (ReferenceEquals(_transaction, transaction)) _transaction = null;
            }
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_transaction is not null)
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        await _context.DisposeAsync();
        GC.SuppressFinalize(this);
    }
}
