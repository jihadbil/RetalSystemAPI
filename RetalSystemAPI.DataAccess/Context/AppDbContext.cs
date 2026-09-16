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

/// <summary>
/// سياق قاعدة البيانات الرئيسي لنظام رتال (Entity Framework Core DbContext).
/// يدير جداول الهوية (Identity)، وجميع جداول الأصناف، المخازن، المبيعات، والمشتريات مع تطبيق العزل التلقائي متعدد المستأجرين (Global Query Filters).
/// </summary>
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentTenantService? _tenantService;

    /// <summary>
    /// تهيئة سياق قاعدة البيانات مع تمرير الخيارات وحقن خدمة المستأجر الحالي.
    /// </summary>
    /// <param name="options">خيارات تكوين DbContext</param>
    /// <param name="tenantService">خدمة جلب معرف المستأجر الحالي</param>
    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentTenantService? tenantService = null) : base(options)
    {
        _tenantService = tenantService;
    }

    /// <summary>
    /// معرف المستأجر الحالي المرتبط بالجلسة أو Guid.Empty.
    /// </summary>
    public Guid CurrentTenantId => _tenantService?.TenantId ?? Guid.Empty;

    // ── DbSets ───────────────────────────────────────────────

    /// <summary>جدول المستأجرين</summary>
    public DbSet<Tenant> Tenants => Set<Tenant>();

    /// <summary>جدول الفروع</summary>
    public DbSet<Branch> Branches => Set<Branch>();

    /// <summary>جدول هواتف الفروع</summary>
    public DbSet<BranchPhone> BranchPhones => Set<BranchPhone>();

    /// <summary>جدول التصنيفات</summary>
    public DbSet<Category> Categories => Set<Category>();

    /// <summary>جدول الأصناف والمنتجات</summary>
    public DbSet<Product> Products => Set<Product>();

    /// <summary>جدول وحدات القياس والتعبئة للأصناف</summary>
    public DbSet<ProductUnit> ProductUnits => Set<ProductUnit>();

    /// <summary>جدول باركودات ونكهات الأصناف</summary>
    public DbSet<ProductBarCode> ProductBarCodes => Set<ProductBarCode>();

    /// <summary>جدول صور الأصناف</summary>
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();

    /// <summary>جدول الوحدات الأساسية</summary>
    public DbSet<Unit> Units => Set<Unit>();

    /// <summary>جدول الموردين</summary>
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    /// <summary>جدول هواتف الموردين</summary>
    public DbSet<SupplierPhone> SupplierPhones => Set<SupplierPhone>();

    /// <summary>جدول المخازن والمستودعات</summary>
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();

    /// <summary>جدول أرصدة المستودع لكل نكهة وباركود</summary>
    public DbSet<StorgeStock> StorgeStocks => Set<StorgeStock>();

    /// <summary>جدول أرصدة صالة العرض لكل صنف</summary>
    public DbSet<ShowroomStock> ShowroomStocks => Set<ShowroomStock>();

    /// <summary>جدول طلبات الشراء</summary>
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();

    /// <summary>جدول بنود طلبات الشراء</summary>
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();

    /// <summary>جدول فواتير المشتريات</summary>
    public DbSet<PurchaseInvoice> PurchaseInvoices => Set<PurchaseInvoice>();

    /// <summary>جدول بنود فواتير المشتريات المجمعة</summary>
    public DbSet<PurchaseInvoiceItem> PurchaseInvoiceItems => Set<PurchaseInvoiceItem>();

    /// <summary>جدول تفصيل النكهات المستلمة في فواتير المشتريات</summary>
    public DbSet<PurchaseInvoiceItemBreakdown> PurchaseInvoiceItemBreakdowns => Set<PurchaseInvoiceItemBreakdown>();

    /// <summary>جدول فواتير مرتجع المشتريات</summary>
    public DbSet<PurchaseReturn> PurchaseReturns => Set<PurchaseReturn>();

    /// <summary>جدول بنود فواتير مرتجع المشتريات</summary>
    public DbSet<PurchaseReturnItem> PurchaseReturnItems => Set<PurchaseReturnItem>();

    /// <summary>جدول الزبائن</summary>
    public DbSet<Customer> Customers => Set<Customer>();

    /// <summary>جدول هواتف الزبائن</summary>
    public DbSet<CustomerPhone> CustomerPhones => Set<CustomerPhone>();

    /// <summary>جدول فواتير المبيعات</summary>
    public DbSet<SalesInvoice> SalesInvoices => Set<SalesInvoice>();

    /// <summary>جدول بنود فواتير المبيعات</summary>
    public DbSet<SalesInvoiceItem> SalesInvoiceItems => Set<SalesInvoiceItem>();

    /// <summary>جدول فواتير مرتجع المبيعات</summary>
    public DbSet<SalesReturn> SalesReturns => Set<SalesReturn>();

    /// <summary>جدول بنود مرتجع المبيعات</summary>
    public DbSet<SalesReturnItem> SalesReturnItems => Set<SalesReturnItem>();

    /// <summary>جدول أوامر التحويل المخزني</summary>
    public DbSet<StockTransfer> StockTransfers => Set<StockTransfer>();

    /// <summary>جدول بنود التحويل المخزني</summary>
    public DbSet<StockTransferItem> StockTransferItems => Set<StockTransferItem>();

    /// <summary>جدول تسويات الجرد المخزني</summary>
    public DbSet<StockAdjustment> StockAdjustments => Set<StockAdjustment>();

    /// <summary>جدول بنود تسويات الجرد المخزني</summary>
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
