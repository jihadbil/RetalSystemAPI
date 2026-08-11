using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Purchase;
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

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // تطبيق جميع إعدادات Fluent API تلقائياً من التجميع الحالية
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // ── Global Query Filters (Soft Delete + Multi-Tenancy) ───────
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                
                // 1. Soft Delete Filter: !e.IsDeleted
                var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var compareIsDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                
                Expression filterExpression = compareIsDeleted;

                // 2. Multi-Tenant Filter: e.TenantId == CurrentTenantId (لكافة الكيانات عدا المستأجر نفسه)
                if (entityType.ClrType != typeof(Tenant))
                {
                    var tenantIdProperty = Expression.Property(parameter, nameof(BaseEntity.TenantId));
                    var currentTenantIdProperty = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));

                    var compareTenantId = Expression.Equal(tenantIdProperty, currentTenantIdProperty);
                    filterExpression = Expression.AndAlso(filterExpression, compareTenantId);
                }

                var lambda = Expression.Lambda(filterExpression, parameter);
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }
}
