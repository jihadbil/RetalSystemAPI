# خطة بناء طبقة DataAccess — DbContext & Repository Pattern
### ✅ القرارات المعتمدة

| القرار | الاختيار |
|---|---|
| موقع DbContext | `RetalSystemAPI.DataAccess` |
| استراتيجية Multi-Tenancy | عزل تام عبر **Global Query Filters** |
| نطاق البناء | النماذج الحالية فقط (بدون Customers/Suppliers/Warehouse) |
| نمط Repository | **Generic Repository** فقط |
| Unit of Work | ✅ نعم |

---

## نظرة عامة على النماذج الحالية

| الكيان | المجلد | الوضع |
|---|---|---|
| `BaseEntity` | `Common/` | ✅ — يحتاج إضافة TenantId |
| `Tenant` | `/` | ✅ جاهز |
| `ApplicationUser` | `/` | ✅ جاهز |
| `Branch` | `Branchs/` | ✅ — يحتاج إزالة TenantId المكرر |
| `BranchPhone` | `Branchs/` | ✅ — يحتاج إزالة TenantId المكرر |
| `Category` | `Catalog/` | ✅ — يحتاج إزالة TenantId المكرر |
| `Product` | `Catalog/` | ✅ — يحتاج إزالة TenantId المكرر |
| `ProductUnit` | `Catalog/` | ✅ — يحتاج إزالة TenantId المكرر |
| `ProductBarCode` | `Catalog/` | ✅ — يحتاج إزالة TenantId المكرر |
| `ProductImage` | `Catalog/` | ✅ — يحتاج إزالة TenantId المكرر |
| `Unit` | `Catalog/` | ✅ — يحتاج إزالة TenantId المكرر |
| `Enums` (4 ملفات) | `Enums/` | ⚠️ هياكل فارغة تحتاج قيماً |

---

## المرحلة الأولى — إصلاح طبقة النماذج

> [!WARNING]
> يجب تطبيق هذه الإصلاحات **أولاً** قبل أي خطوة أخرى

---

#### [MODIFY] [BaseEntity.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Common/BaseEntity.cs)
نقل `TenantId` و`Tenant` إلى الكيان الأساسي لإزالة التكرار من 8 نماذج:

```csharp
namespace RetalSystemAPI.Models.Common;

public abstract class BaseEntity
{
    public Guid Id { get; set; }

    // ── Multi-Tenancy ────────────────────────────────────
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    // ── Audit Fields ─────────────────────────────────────
    public DateTime  CreatedAt        { get; set; }
    public DateTime? UpdatedAt        { get; set; }
    public string?   CreatedByUserId  { get; set; }
    public string?   UpdatedByUserId  { get; set; }

    // ── Soft Delete ───────────────────────────────────────
    public bool IsDeleted { get; set; } = false;

    // ── Optimistic Concurrency ────────────────────────────
    public byte[] RowVersion { get; set; } = null!;
}
```

---

#### [NEW] [ValidationConstants.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Common/ValidationConstants.cs)
استخراج Regex الهاتف الليبي في ثابت مشترك:

```csharp
namespace RetalSystemAPI.Models.Common;

public static class ValidationConstants
{
    public const string LibyanPhonePattern = @"^(09\d{8}|\+2189\d{8})$";
    public const string LibyanPhoneError   = "رقم الهاتف غير صحيح. يجب أن يبدأ بـ 09 أو +2189";
}
```

---

#### [MODIFY] الـ Enums الأربعة — إضافة قيم حقيقية

```csharp
// InvoiceStatus.cs
public enum InvoiceStatus
{
    Draft = 0,
    Pending = 1,
    Paid = 2,
    PartiallyPaid = 3,
    Cancelled = 4,
    Voided = 5
}

// PaymentStatus.cs
public enum PaymentStatus
{
    Pending = 0,
    Completed = 1,
    Failed = 2,
    Refunded = 3,
    PartiallyRefunded = 4
}

// SalesOrderStatus.cs
public enum SalesOrderStatus
{
    Draft = 0,
    Confirmed = 1,
    Processing = 2,
    Shipped = 3,
    Delivered = 4,
    Cancelled = 5,
    Returned = 6
}

// WarehouseType.cs
public enum WarehouseType
{
    Main = 0,
    Branch = 1,
    Transit = 2,
    Consignment = 3,
    Virtual = 4
}
```

---

#### [MODIFY] إزالة TenantId المكرر من 8 نماذج
يُزال `public Guid TenantId { get; set; }` و `public Tenant? Tenant { get; set; }` من:
`Branch`, `BranchPhone`, `Category`, `Product`, `ProductUnit`, `ProductBarCode`, `ProductImage`, `Unit`

---

#### [MODIFY] تحديث Regex في Tenant.cs و BranchPhone.cs
استبدال الـ Regex المكرر باستخدام `ValidationConstants.LibyanPhonePattern`

---

## المرحلة الثانية — هيكل ملفات DataAccess

```
RetalSystemAPI.DataAccess/
│
├── Context/
│   ├── AppDbContext.cs
│   └── Configurations/
│       ├── TenantConfiguration.cs
│       ├── ApplicationUserConfiguration.cs
│       ├── BranchConfiguration.cs
│       ├── BranchPhoneConfiguration.cs
│       ├── CategoryConfiguration.cs
│       ├── ProductConfiguration.cs
│       ├── ProductUnitConfiguration.cs
│       ├── ProductBarCodeConfiguration.cs
│       ├── ProductImageConfiguration.cs
│       └── UnitConfiguration.cs
│
├── Repositories/
│   ├── Interfaces/
│   │   ├── IRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Implementations/
│       ├── Repository.cs
│       └── UnitOfWork.cs
│
├── Interceptors/
│   └── AuditInterceptor.cs
│
├── Services/
│   └── CurrentTenantService.cs
│
└── Extensions/
    └── DataAccessServiceExtensions.cs
```

---

## المرحلة الثالثة — AppDbContext

#### [NEW] [AppDbContext.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/AppDbContext.cs)

```csharp
public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    private readonly ICurrentTenantService _tenantService;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        ICurrentTenantService tenantService) : base(options)
    {
        _tenantService = tenantService;
    }

    // ── DbSets ───────────────────────────────────────────────
    public DbSet<Tenant>         Tenants         => Set<Tenant>();
    public DbSet<Branch>         Branches        => Set<Branch>();
    public DbSet<BranchPhone>    BranchPhones    => Set<BranchPhone>();
    public DbSet<Category>       Categories      => Set<Category>();
    public DbSet<Product>        Products        => Set<Product>();
    public DbSet<ProductUnit>    ProductUnits    => Set<ProductUnit>();
    public DbSet<ProductBarCode> ProductBarCodes => Set<ProductBarCode>();
    public DbSet<ProductImage>   ProductImages   => Set<ProductImage>();
    public DbSet<Unit>           Units           => Set<Unit>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // تطبيق Fluent API Configurations تلقائياً
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // ── Global Filters: Soft Delete + Multi-Tenant ───────
        var tenantId = _tenantService.TenantId;

        builder.Entity<Branch>()
            .HasQueryFilter(e => !e.IsDeleted && e.TenantId == tenantId);
        // ... (يتكرر لكل كيان يرث BaseEntity)
    }
}
```

---

## المرحلة الرابعة — Fluent API Configurations

### TenantConfiguration
```csharp
builder.ToTable("Tenants");
builder.HasKey(t => t.Id);
builder.Property(t => t.Name).IsRequired().HasMaxLength(200);
builder.Property(t => t.PhoneNumber).IsRequired().HasMaxLength(20);
builder.HasIndex(t => t.Name).IsUnique();
builder.Property(t => t.RowVersion).IsRowVersion().IsConcurrencyToken();
```

### ProductConfiguration — أهم الإعدادات
```csharp
// دقة مالية أساسية
builder.Property(p => p.CostPrice).HasColumnType("decimal(18,4)");
builder.Property(p => p.SalePrice).HasColumnType("decimal(18,4)");
builder.Property(p => p.AveragePrice).HasColumnType("decimal(18,4)");

// مؤشرات الأداء
builder.HasIndex(p => p.TenantId);
builder.HasIndex(p => new { p.TenantId, p.CategoryId });
builder.HasIndex(p => new { p.TenantId, p.IsDeleted });

// العلاقات (بدون حذف تسلسلي)
builder.HasOne(p => p.Category)
    .WithMany(c => c.Products)
    .HasForeignKey(p => p.CategoryId)
    .OnDelete(DeleteBehavior.Restrict);
```

### جدول إعدادات الكيانات

| الكيان | MaxLength | Indexes | OnDelete |
|---|---|---|---|
| `Tenant` | Name:200, Phone:20 | Unique(Name) | — |
| `Branch` | Name:150, Address:500 | (TenantId), (TenantId,IsActive) | Restrict |
| `BranchPhone` | Phone:20, Name:100 | Unique(TenantId,PhoneNumber) | Cascade |
| `Category` | Name:150 | (TenantId), (TenantId,ParentId) | Restrict |
| `Product` | Name:200 | (TenantId,CategoryId), (TenantId,IsDeleted) | Restrict |
| `ProductBarCode` | BarCode:100, Title:300 | Unique(TenantId,BarCode) | Cascade |
| `ProductImage` | ImageUrl:1000 | (ProductId), (BarcodeId) | Cascade |
| `ProductUnit` | — | Unique(ProductId,UnitId,TenantId) | Cascade |
| `Unit` | Name:100 | (TenantId), Unique(TenantId,Name) | Restrict |

---

## المرحلة الخامسة — Generic Repository

#### [NEW] [IRepository.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Repositories/Interfaces/IRepository.cs)

```csharp
public interface IRepository<T> where T : BaseEntity
{
    // ── Queries ──────────────────────────────────────────────
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);
    Task<T?> FirstOrDefaultAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);
    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken ct = default);
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken ct = default);

    // ── Pagination ────────────────────────────────────────────
    Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>?    filter    = null,
        Expression<Func<T, object>>?  orderBy   = null,
        bool ascending = true,
        CancellationToken ct = default);

    // ── Commands ──────────────────────────────────────────────
    Task AddAsync(T entity, CancellationToken ct = default);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken ct = default);
    void Update(T entity);
    void SoftDelete(T entity);      // يضبط IsDeleted = true فقط
    void HardDelete(T entity);      // حذف فعلي من قاعدة البيانات
}
```

---

## المرحلة السادسة — Unit of Work

#### [NEW] [IUnitOfWork.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Repositories/Interfaces/IUnitOfWork.cs)

```csharp
public interface IUnitOfWork : IAsyncDisposable
{
    // ── Repositories ──────────────────────────────────────────
    IRepository<Tenant>         Tenants         { get; }
    IRepository<Branch>         Branches        { get; }
    IRepository<BranchPhone>    BranchPhones    { get; }
    IRepository<Category>       Categories      { get; }
    IRepository<Product>        Products        { get; }
    IRepository<ProductUnit>    ProductUnits    { get; }
    IRepository<ProductBarCode> ProductBarCodes { get; }
    IRepository<ProductImage>   ProductImages   { get; }
    IRepository<Unit>           Units           { get; }

    // ── Transaction Control ───────────────────────────────────
    Task<int> SaveChangesAsync(CancellationToken ct = default);
    Task      BeginTransactionAsync(CancellationToken ct = default);
    Task      CommitTransactionAsync(CancellationToken ct = default);
    Task      RollbackTransactionAsync(CancellationToken ct = default);
}
```

---

## المرحلة السابعة — AuditInterceptor

#### [NEW] [AuditInterceptor.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Interceptors/AuditInterceptor.cs)

```csharp
public class AuditInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUser;

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is null) return base.SavingChangesAsync(eventData, result, ct);

        foreach (var entry in eventData.Context.ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.Id              = entry.Entity.Id == Guid.Empty
                                                    ? Guid.NewGuid()
                                                    : entry.Entity.Id;
                    entry.Entity.CreatedAt       = DateTime.UtcNow;
                    entry.Entity.CreatedByUserId = _currentUser.UserId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt       = DateTime.UtcNow;
                    entry.Entity.UpdatedByUserId = _currentUser.UserId;
                    break;
            }
        }
        return base.SavingChangesAsync(eventData, result, ct);
    }
}
```

---

## المرحلة الثامنة — CurrentTenantService

```csharp
// Services/ICurrentTenantService.cs
public interface ICurrentTenantService
{
    Guid TenantId { get; }
}

// Services/CurrentTenantService.cs
public class CurrentTenantService : ICurrentTenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public Guid TenantId
    {
        get
        {
            var claim = _httpContextAccessor.HttpContext?
                .User.FindFirst("TenantId")?.Value;

            return Guid.TryParse(claim, out var id) 
                ? id 
                : throw new UnauthorizedAccessException("TenantId غير موجود في التوكن");
        }
    }
}
```

---

## المرحلة التاسعة — DI Registration

#### [NEW] [DataAccessServiceExtensions.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Extensions/DataAccessServiceExtensions.cs)

```csharp
public static class DataAccessServiceExtensions
{
    public static IServiceCollection AddDataAccess(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── CurrentTenant Service ─────────────────────────────
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentTenantService, CurrentTenantService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // ── DbContext ─────────────────────────────────────────
        services.AddDbContext<AppDbContext>((sp, options) =>
        {
            options
                .UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    sql => sql.MigrationsAssembly(
                        typeof(AppDbContext).Assembly.FullName)
                )
                .AddInterceptors(sp.GetRequiredService<AuditInterceptor>());
        });

        // ── Interceptors ──────────────────────────────────────
        services.AddScoped<AuditInterceptor>();

        // ── Repositories & UnitOfWork ─────────────────────────
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        return services;
    }
}
```

---

## خريطة العلاقات (ERD)

```
Tenant ──(1:N)──► Branch ──(1:N)──► BranchPhone
  │                  │
  │                  └──(1:N)──► ApplicationUser
  │
  └──(1:N)──► Category ──(Self: 1:N)──► SubCategories
  │              │
  │              └──(1:N)──► Product
  │                             ├──(1:N)──► ProductUnit ──► Unit
  │                             ├──(1:N)──► ProductBarCode
  │                             │               └──(1:N)──► ProductImage
  │                             └──(1:N)──► ProductImage (عامة)
  │
  └──(1:N)──► Unit
```

---

## Verification Plan

### بناء والـ Migration
```bash
# بناء المشروع
dotnet build RetalSystemAPI.DataAccess

# إنشاء أول Migration
dotnet ef migrations add InitialCreate \
  --project RetalSystemAPI.DataAccess \
  --startup-project RetalSystemAPI

# مراجعة الـ Migration المنشأة
# تطبيق على قاعدة البيانات
dotnet ef database update \
  --project RetalSystemAPI.DataAccess \
  --startup-project RetalSystemAPI
```

### اختبارات يدوية بعد البناء
- [ ] Global Query Filter يمنع رؤية بيانات مستأجر آخر
- [ ] Soft Delete لا يُظهر السجلات المحذوفة في الاستعلامات
- [ ] AuditInterceptor يملأ `CreatedAt` و`CreatedByUserId` عند الإضافة
- [ ] `RowVersion` يمنع التحديثات المتعارضة (Optimistic Concurrency)
- [ ] Unique Indexes على BarCode و PhoneNumber تمنع التكرار

---

## ترتيب التنفيذ

```
Phase 1  ─ إصلاح Models (BaseEntity + Enums + Regex)         [~30 min]
Phase 2  ─ هيكل مجلدات DataAccess                           [~5 min]
Phase 3  ─ ICurrentTenantService + ICurrentUserService       [~15 min]
Phase 4  ─ Fluent API Configurations (10 ملفات)             [~40 min]
Phase 5  ─ AppDbContext مع Global Query Filters              [~20 min]
Phase 6  ─ IRepository Interface                             [~10 min]
Phase 7  ─ Repository<T> Implementation                      [~20 min]
Phase 8  ─ IUnitOfWork Interface + UnitOfWork Implementation [~20 min]
Phase 9  ─ AuditInterceptor                                  [~15 min]
Phase 10 ─ DI Registration Extension                        [~10 min]
Phase 11 ─ Migration + Verification                         [~15 min]
─────────────────────────────────────────────────────────────
Total:  ~3 ساعات
```
