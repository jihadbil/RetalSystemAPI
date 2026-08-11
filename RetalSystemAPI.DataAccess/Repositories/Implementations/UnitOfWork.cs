using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Repositories.Implementations;

/// <summary>
/// التنفيذ الأحدث لـ Unit of Work الذي يدير المعاملات الموحدة والمستودعات العامة.
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
    private IRepository<PurchaseOrder>? _purchaseOrders;
    private IRepository<PurchaseOrderItem>? _purchaseOrderItems;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<Tenant> Tenants => _tenants ??= new Repository<Tenant>(_context);
    public IRepository<Branch> Branches => _branches ??= new Repository<Branch>(_context);
    public IRepository<BranchPhone> BranchPhones => _branchPhones ??= new Repository<BranchPhone>(_context);
    public IRepository<Category> Categories => _categories ??= new Repository<Category>(_context);
    public IRepository<Product> Products => _products ??= new Repository<Product>(_context);
    public IRepository<ProductUnit> ProductUnits => _productUnits ??= new Repository<ProductUnit>(_context);
    public IRepository<ProductBarCode> ProductBarCodes => _productBarCodes ??= new Repository<ProductBarCode>(_context);
    public IRepository<ProductImage> ProductImages => _productImages ??= new Repository<ProductImage>(_context);
    public IRepository<Unit> Units => _units ??= new Repository<Unit>(_context);
    public IRepository<Supplier> Suppliers => _suppliers ??= new Repository<Supplier>(_context);
    public IRepository<SupplierPhone> SupplierPhones => _supplierPhones ??= new Repository<SupplierPhone>(_context);
    public IRepository<Warehouse> Warehouses => _warehouses ??= new Repository<Warehouse>(_context);
    public IRepository<StorgeStock> StorgeStocks => _storgeStocks ??= new Repository<StorgeStock>(_context);
    public IRepository<ShowroomStock> ShowroomStocks => _showroomStocks ??= new Repository<ShowroomStock>(_context);
    public IRepository<PurchaseOrder> PurchaseOrders => _purchaseOrders ??= new Repository<PurchaseOrder>(_context);
    public IRepository<PurchaseOrderItem> PurchaseOrderItems => _purchaseOrderItems ??= new Repository<PurchaseOrderItem>(_context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }

    public System.Collections.Generic.IEnumerable<Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry> ChangeTrackerEntries()
    {
        return _context.ChangeTracker.Entries();
    }

    public async Task BeginTransactionAsync(CancellationToken ct = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null)
        {
            throw new InvalidOperationException("لا توجد معاملة مفعلة حالياً ليتم اعتمادها (Commit).");
        }

        try
        {
            await _context.SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);
        }
        catch
        {
            await RollbackTransactionAsync(ct);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken ct = default)
    {
        if (_transaction is null) return;

        try
        {
            await _transaction.RollbackAsync(ct);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

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
