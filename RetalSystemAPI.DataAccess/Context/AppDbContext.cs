using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Customers;
using RetalSystemAPI.Models.Purchase;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Models.Suppliers;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.DataAccess.Context;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentTenantService? _tenantService;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentTenantService? tenantService = null) : base(options)
    {
        _tenantService = tenantService;
    }

    public Guid CurrentTenantId => _tenantService?.TenantId ?? Guid.Empty;

    // ── DbSets ───────────────────────────────────────────────
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<BranchPhone> BranchPhones => Set<BranchPhone>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductUnit> ProductUnits => Set<ProductUnit>();
    public DbSet<ProductBarCode> ProductBarCodes => Set<ProductBarCode>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Unit> Units => Set<Unit>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<SupplierPhone> SupplierPhones => Set<SupplierPhone>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StorgeStock> StorgeStocks => Set<StorgeStock>();
    public DbSet<ShowroomStock> ShowroomStocks => Set<ShowroomStock>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<CustomerPhone> CustomerPhones => Set<CustomerPhone>();
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();
    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();
    public DbSet<SalesReturnItem> SalesReturnItems => Set<SalesReturnItem>();
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();
    public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();
    public DbSet<StockAdjustmentItem> StockAdjustmentItems => Set<StockAdjustmentItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // تطبيق جميع إعدادات Fluent API تلقائياً من التجميع الحالية
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // ── Global Query Filters (Soft Delete + Multi-Tenancy) ───────
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (entityType.ClrType == typeof(Tenant))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                
                // Soft Delete Filter: !e.IsDeleted
                var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var compareIsDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                var lambda = Expression.Lambda(compareIsDeleted, parameter);
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
            else if (typeof(TenantBaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                
                // 1. Soft Delete Filter: !e.IsDeleted
                var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var compareIsDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                
                // 2. Multi-Tenant Filter: e.TenantId == CurrentTenantId
                var tenantIdProperty = Expression.Property(parameter, nameof(TenantBaseEntity.TenantId));
                var currentTenantIdProperty = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
                var compareTenantId = Expression.Equal(tenantIdProperty, currentTenantIdProperty);

                var filterExpression = Expression.AndAlso(compareIsDeleted, compareTenantId);

                var lambda = Expression.Lambda(filterExpression, parameter);
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
            else if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                
                // Soft Delete Filter: !e.IsDeleted
                var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var compareIsDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                var lambda = Expression.Lambda(compareIsDeleted, parameter);
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndTenantInfo();
        return await base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditAndTenantInfo();
        return base.SaveChanges();
    }

    private void ApplyAuditAndTenantInfo()
    {
        var tenantId = CurrentTenantId;
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity is TenantBaseEntity tenantEntity && tenantEntity.TenantId == Guid.Empty && tenantId != Guid.Empty)
                {
                    tenantEntity.TenantId = tenantId;
                }

                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = now;
                }
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
