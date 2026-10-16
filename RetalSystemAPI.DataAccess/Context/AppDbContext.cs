using System;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.Accounting;
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
    private bool _transactionFailed;

    internal void EnsureTransactionUsable()
    {
        if (_transactionFailed)
            throw new InvalidOperationException("تم التراجع عن المعاملة. أنشئ سياق بيانات جديداً قبل متابعة الكتابة.");
    }

    internal void InvalidateAfterRollback()
    {
        _transactionFailed = true;
        ChangeTracker.Clear();
    }

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

    /// <summary>جدول الخزائن والبنوك وأدراج النقود</summary>
    public DbSet<Safe> Safes => Set<Safe>();

    /// <summary>جدول حركات الخزائن والبنوك وأدراج النقود</summary>
    public DbSet<SafeTransaction> SafeTransactions => Set<SafeTransaction>();

    /// <summary>جدول تصنيفات المصروفات</summary>
    public DbSet<ExpenseCategory> ExpenseCategories => Set<ExpenseCategory>();

    /// <summary>جدول المصروفات</summary>
    public DbSet<Expense> Expenses => Set<Expense>();

    /// <summary>جدول حركات كشف حساب الموردين</summary>
    public DbSet<SupplierTransaction> SupplierTransactions => Set<SupplierTransaction>();

    /// <summary>جدول حركات كشف حساب الزبائن والعملاء</summary>
    public DbSet<CustomerTransaction> CustomerTransactions => Set<CustomerTransaction>();

    public DbSet<PosTerminal> PosTerminals => Set<PosTerminal>();
    public DbSet<PosDeviceCredential> PosDeviceCredentials => Set<PosDeviceCredential>();
    public DbSet<CashSession> CashSessions => Set<CashSession>();

    /// <summary>
    /// تكوين العلاقات، ومفاتيح الجداول، وتطبيق مرشحات الاستعلام العامة للعزل المتعدد للمستأجرين والحذف الناعم.
    /// </summary>
    /// <param name="builder">منشئ نماذج كيانات قاعدة البيانات ModelBuilder</param>
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // استدعاء البنية الأساسية لـ IdentityDbContext لتهيئة جداول المستخدمين والصلاحيات
        base.OnModelCreating(builder);

        // اكتشاف وتطبيق كافة فئات التكوين (IEntityTypeConfiguration) في هذا التجميع تلقائياً
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        ModelIntegrityConfiguration.Apply(builder);

        // المرور على كافة الكيانات المسجلة في النموذج لفحص شجرة وراثتها وتطبيق مرشحات الأمان
        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            // فحص إذا كان الكيان هو جدول المستأجرين نفسه (Tenant)
            if (entityType.ClrType == typeof(Tenant))
            {
                // إنشاء معامل التعبير البرمجي للكيان (e => ...)
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                
                // استخراج خاصية الحذف الناعم IsDeleted من الكيان
                var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                // بناء شرط استبعاد المحذوفات: e.IsDeleted == false
                var compareIsDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                // بناء دالة Lambda للشرط: e => e.IsDeleted == false
                var lambda = Expression.Lambda(compareIsDeleted, parameter);
                // تطبيق مرشح الاستعلام العام على جدول المستأجرين
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
            // فحص إذا كان الكيان يتبع مستأجراً معيناً ويرث من TenantBaseEntity
            else if (typeof(TenantBaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // إنشاء معامل التعبير البرمجي للكيان (e => ...)
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                
                // 1. استخراج خاصية الحذف الناعم وبناء شرط الفلترة: e.IsDeleted == false
                var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var compareIsDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));
                
                // 2. استخراج خاصية معرف المستأجر من الكيان ومعرف المستأجر الحالي من السياق
                var tenantIdProperty = Expression.Property(parameter, nameof(TenantBaseEntity.TenantId));
                var currentTenantIdProperty = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
                // بناء شرط تطابق المستأجر: e.TenantId == CurrentTenantId
                var compareTenantId = Expression.Equal(tenantIdProperty, currentTenantIdProperty);

                // دمج الشرطين معاً منطقياً: !e.IsDeleted && e.TenantId == CurrentTenantId
                var filterExpression = Expression.AndAlso(compareIsDeleted, compareTenantId);

                // بناء دالة Lambda المركبة
                var lambda = Expression.Lambda(filterExpression, parameter);
                // تطبيق مرشح الاستعلام المزدوج للعزل التام والأمان على الكيان
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
            // فحص إذا كان الكيان مشتركاً أو عاماً ويرث فقط من BaseEntity دون تخصيص مستأجر
            else if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // إنشاء معامل التعبير البرمجي للكيان (e => ...)
                var parameter = Expression.Parameter(entityType.ClrType, "e");
                
                // استخراج خاصية الحذف الناعم وبناء شرط الاستبعاد: e.IsDeleted == false
                var isDeletedProperty = Expression.Property(parameter, nameof(BaseEntity.IsDeleted));
                var compareIsDeleted = Expression.Equal(isDeletedProperty, Expression.Constant(false));

                // بناء دالة Lambda وتطبيق مرشح الحذف الناعم على الكيان العام
                var lambda = Expression.Lambda(compareIsDeleted, parameter);
                builder.Entity(entityType.ClrType).HasQueryFilter(lambda);
            }
        }
    }

    /// <summary>
    /// حفظ كافة التغييرات المعلقة في سياق البيانات بشكل غير متزامن مع تطبيق بيانات التدقيق وتعيين المستأجر.
    /// </summary>
    /// <param name="cancellationToken">رمز إلغاء العملية غير المتزامنة</param>
    /// <returns>عدد السجلات التي تأثرت بعملية الحفظ في قاعدة البيانات</returns>
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => SaveChangesAsync(true, cancellationToken);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        // استدعاء دالة حقن معلومات التدقيق والتواريخ ومعرف المستأجر
        ApplyAuditAndTenantInfo();
        // تمرير الحفظ لمحرك EF Core الأساسي
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    /// <summary>
    /// حفظ كافة التغييرات المعلقة في سياق البيانات بشكل تزامني مع تطبيق بيانات التدقيق وتعيين المستأجر.
    /// </summary>
    /// <returns>عدد السجلات التي تأثرت بعملية الحفظ في قاعدة البيانات</returns>
    public override int SaveChanges() => SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        // استدعاء دالة حقن معلومات التدقيق والتواريخ ومعرف المستأجر
        ApplyAuditAndTenantInfo();
        // تمرير الحفظ لمحرك EF Core الأساسي
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    /// <summary>
    /// فحص الكيانات المتتبعة وتحديث حقول التدقيق (تاريخ الإنشاء والتعديل) ومعرف المستأجر تلقائياً.
    /// </summary>
    private void ApplyAuditAndTenantInfo()
    {
        EnsureTransactionUsable();
        // جلب معرف المستأجر الحالي المرتبط بالجلسة الحالية
        var tenantId = CurrentTenantId;
        // تحديد التوقيت الحالي بتوقيت جرينتش UTC لضمان الدقة
        var now = DateTime.UtcNow;

        // المرور على كافة الكيانات المتتبعة في ChangeTracker والتي ترث من BaseEntity
        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is TenantBaseEntity owned && entry.Entity is not Tenant
                && entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            {
                if (entry.State == EntityState.Added && owned.TenantId == Guid.Empty)
                    owned.TenantId = tenantId;

                var originalTenantId = entry.Property(nameof(TenantBaseEntity.TenantId)).OriginalValue;
                if (owned.TenantId == Guid.Empty
                    || (tenantId != Guid.Empty && owned.TenantId != tenantId)
                    || (entry.State != EntityState.Added && !Equals(originalTenantId, owned.TenantId)))
                    throw new UnauthorizedAccessException("لا يمكن حفظ بيانات تخص مستأجراً آخر أو تغيير مالك السجل.");
            }

            // في حالة إضافة سجل جديد تماماً
            if (entry.State == EntityState.Added)
            {
                // إذا كان الكيان يتبع لمستأجر وكان المعرف فارغاً، يتم تعيين معرف المستأجر الحالي تلقائياً
                if (entry.Entity is TenantBaseEntity tenantEntity && tenantEntity.TenantId == Guid.Empty && tenantId != Guid.Empty)
                {
                    tenantEntity.TenantId = tenantId;
                }

                // تعيين تاريخ الإنشاء إذا لم يتم تعيينه مسبقاً
                if (entry.Entity.CreatedAt == default)
                {
                    entry.Entity.CreatedAt = now;
                }
            }
            // في حالة تعديل سجل موجود مسبقاً
            else if (entry.State == EntityState.Modified)
            {
                // تحديث تاريخ التعديل إلى الوقت الحالي
                entry.Entity.UpdatedAt = now;
            }
        }
    }
}
