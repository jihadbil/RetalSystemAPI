# خطة بناء طبقة الخدمات — RetalSystemAPI.Services

## القرارات المعتمدة ✅

| الموضوع | القرار |
|---------|--------|
| سياسة JWT | Access Token فقط — بدون Refresh Token (بيئة اختبار) |
| رفع الملفات | تخزين محلي في `wwwroot/uploads` |
| Include Strategy | **Specification Pattern** — تعريف الـ Includes خارج الريبوستري |
| Pagination | Query String موحد: `?page=1&size=20` |
| أكواد الخطأ | نظام أكواد موحد (`PRODUCT_NOT_FOUND`, `BARCODE_DUPLICATE`, ...) |

---

## نظرة عامة

بعد التحليل العميق لطبقة النماذج (Models) وطبقة الوصول للبيانات (DataAccess)، تبيّن أن المشروع يمتلك بنية تحتية متينة جداً تشمل:

- **BaseEntity**: مع دعم Multi-Tenancy، Soft Delete، Audit Fields، RowVersion
- **IRepository\<T\>**: Generic Repository مع Pagination، Find، Exists، Count
- **IUnitOfWork**: يغطي 9 مستودعات مع Transaction Control كامل
- **AutoMapper Profiles**: جاهزة للـ Catalog والـ Branch والـ Tenant
- **AuditInterceptor**: يتتبع CreatedAt، UpdatedAt، CreatedByUserId تلقائياً
- **Global Query Filters**: تفرض Soft Delete + Multi-Tenancy تلقائياً على مستوى EF Core

طبقة الخدمات (**Services Layer**) هي الطبقة التي تحتل المنتصف بين المستودعات وـ Controllers، وتحمل كامل منطق الأعمال (Business Logic).

---

## هيكل المجلدات المقترح

```
RetalSystemAPI.Services/
├── Common/
│   ├── Interfaces/
│   │   └── IBaseService.cs
│   ├── Implementations/
│   │   └── BaseService.cs
│   └── Models/
│       ├── PagedResult.cs
│       └── ServiceResult.cs
│
├── Catalog/
│   ├── Interfaces/
│   │   ├── ICategoryService.cs
│   │   ├── IProductService.cs
│   │   ├── IUnitService.cs
│   │   ├── IProductUnitService.cs
│   │   ├── IProductBarCodeService.cs
│   │   └── IProductImageService.cs
│   └── Implementations/
│       ├── CategoryService.cs
│       ├── ProductService.cs
│       ├── UnitService.cs
│       ├── ProductUnitService.cs
│       ├── ProductBarCodeService.cs
│       └── ProductImageService.cs
│
├── Branch/
│   ├── Interfaces/
│   │   └── IBranchService.cs
│   └── Implementations/
│       └── BranchService.cs
│
├── Tenant/
│   ├── Interfaces/
│   │   └── ITenantService.cs
│   └── Implementations/
│       └── TenantService.cs
│
├── Auth/
│   ├── Interfaces/
│   │   └── IAuthService.cs
│   └── Implementations/
│       └── AuthService.cs
│
├── FileUpload/
│   ├── Interfaces/
│   │   └── IFileUploadService.cs
│   └── Implementations/
│       └── FileUploadService.cs
│
└── Extensions/
    └── ServicesLayerExtensions.cs
```

---

## تحليل طبقة النماذج والريبوستري

### ✅ ما يوفره الريبوستري لطبقة الخدمات

| العملية | الدالة المتاحة |
|---------|----------------|
| جلب بالـ ID | `GetByIdAsync(Guid id)` |
| جلب الكل | `GetAllAsync()` |
| بحث مشروط | `FindAsync(Expression predicate)` |
| أول نتيجة | `FirstOrDefaultAsync(Expression predicate)` |
| التحقق من الوجود | `ExistsAsync(Expression predicate)` |
| العدد | `CountAsync(Expression predicate)` |
| صفحات | `GetPagedAsync(pageNumber, pageSize, filter, orderBy, ascending)` |
| إضافة | `AddAsync(entity)` |
| إضافة مجموعة | `AddRangeAsync(entities)` |
| تحديث | `Update(entity)` |
| حذف ناعم | `SoftDelete(entity)` — يعيّن IsDeleted=true |
| حذف فعلي | `HardDelete(entity)` |
| حفظ | `SaveChangesAsync()` |
| معاملة | `BeginTransactionAsync()`, `CommitTransactionAsync()`, `RollbackTransactionAsync()` |

### ⚠️ ملاحظات يجب مراعاتها في طبقة الخدمات

1. **GetAllAsync** لا تدعم Include للعلاقات — الخدمات ستحتاج إلى استراتيجية لـ Eager Loading
2. **RowVersion** موجود في BaseEntity — يجب التعامل معه في عمليات التحديث للحماية من Race Conditions
3. **TenantId** يُحقن تلقائياً من AuditInterceptor — الخدمات لا تحتاج لتعيينه يدوياً
4. **ProductUnit** يربط Product + Unit بعامل تحويل — المنطق يجب أن يتحقق من وحدة افتراضية واحدة فقط
5. **Category** لها هيكل هرمي (ParentCategoryId self-reference) — يجب التحقق من عدم إنشاء حلقة دائرية

---

## التغييرات المقترحة

### 1. ملف المشروع

#### [MODIFY] [RetalSystemAPI.Services.csproj](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Services/RetalSystemAPI.Services.csproj)

إضافة مراجع للمشاريع الأخرى وحزمة AutoMapper:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AutoMapper" Version="14.*" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="10.*" />
    <PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="10.*" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\RetalSystemAPI.DataAccess\RetalSystemAPI.DataAccess.csproj" />
    <ProjectReference Include="..\RetalSystemAPI.Models.DTOs\RetalSystemAPI.Models.DTOs.csproj" />
  </ItemGroup>
</Project>
```

---

### 2. Common — الهياكل المشتركة

#### [NEW] Common/Models/ServiceResult.cs

```csharp
/// <summary>
/// غلاف موحد لجميع نتائج الخدمات، يحمل البيانات أو رسائل الخطأ.
/// يُجنّب استخدام الاستثناءات في حالات الأعمال المتوقعة.
/// </summary>
public class ServiceResult<T>
{
    public bool IsSuccess { get; private set; }
    public T? Data { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorCode { get; private set; }

    public static ServiceResult<T> Success(T data) => new() { IsSuccess = true, Data = data };
    public static ServiceResult<T> Failure(string message, string? code = null) =>
        new() { IsSuccess = false, ErrorMessage = message, ErrorCode = code };
}

// نسخة بدون بيانات للعمليات مثل الحذف
public class ServiceResult
{
    public bool IsSuccess { get; private set; }
    public string? ErrorMessage { get; private set; }
    public string? ErrorCode { get; private set; }

    public static ServiceResult Success() => new() { IsSuccess = true };
    public static ServiceResult Failure(string message, string? code = null) =>
        new() { IsSuccess = false, ErrorMessage = message, ErrorCode = code };
}
```

#### [NEW] Common/Models/PagedResult.cs

```csharp
/// <summary>
/// حاوية موحدة لنتائج الصفحات مع معلومات الترقيم.
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];
    public int TotalCount { get; init; }
    public int PageNumber { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;
}
```

---

### 3. Catalog — الكتالوج

#### [NEW] Catalog/Interfaces/ICategoryService.cs

```csharp
public interface ICategoryService
{
    Task<ServiceResult<CategoryResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetRootCategoriesAsync(CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<CategoryResponseDto>>> GetSubCategoriesAsync(Guid parentId, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<CategoryResponseDto>>> GetPagedAsync(int page, int size, CancellationToken ct = default);
    Task<ServiceResult<CategoryResponseDto>> CreateAsync(CreateCategoryDto dto, CancellationToken ct = default);
    Task<ServiceResult<CategoryResponseDto>> UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
```

**منطق الأعمال (Business Rules) لـ CategoryService:**
- `CreateAsync`: التحقق من عدم تكرار الاسم في نفس المستأجر + التحقق من أن ParentCategoryId موجود إذا مُرر
- `UpdateAsync`: التحقق من عدم إنشاء حلقة دائرية (category لا تكون أبًا لنفسها أو لأحد أجدادها)
- `DeleteAsync`: الحذف الناعم فقط — التحقق من عدم وجود منتجات أو تصنيفات فرعية نشطة

#### [NEW] Catalog/Interfaces/IProductService.cs

```csharp
public interface IProductService
{
    Task<ServiceResult<ProductResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<PagedResult<ProductSummaryDto>>> GetPagedAsync(int page, int size, Guid? categoryId = null, CancellationToken ct = default);
    Task<ServiceResult<ProductResponseDto>> CreateAsync(CreateProductDto dto, CancellationToken ct = default);
    Task<ServiceResult<ProductResponseDto>> UpdateAsync(Guid id, UpdateProductDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<ProductResponseDto>> GetByBarCodeAsync(string barCode, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<ProductSummaryDto>>> SearchAsync(string query, CancellationToken ct = default);
}
```

**منطق الأعمال لـ ProductService:**
- `CreateAsync`: التحقق من أن CategoryId موجود + التحقق من أن CostPrice و SalePrice غير سالبتين
- `UpdateAsync`: التعامل مع RowVersion لمنع التعارض المتزامن (Concurrency Conflict)
- `GetByBarCodeAsync`: البحث عبر مستودع ProductBarCodes بـ BarCode string

#### [NEW] Catalog/Interfaces/IUnitService.cs

```csharp
public interface IUnitService
{
    Task<ServiceResult<UnitResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<UnitResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<UnitResponseDto>> CreateAsync(CreateUnitDto dto, CancellationToken ct = default);
    Task<ServiceResult<UnitResponseDto>> UpdateAsync(Guid id, UpdateUnitDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
}
```

#### [NEW] Catalog/Interfaces/IProductUnitService.cs

```csharp
public interface IProductUnitService
{
    Task<ServiceResult<IReadOnlyList<ProductUnitResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<ServiceResult<ProductUnitResponseDto>> AddUnitToProductAsync(Guid productId, CreateProductUnitDto dto, CancellationToken ct = default);
    Task<ServiceResult> RemoveUnitFromProductAsync(Guid productUnitId, CancellationToken ct = default);
    Task<ServiceResult> SetDefaultUnitAsync(Guid productUnitId, CancellationToken ct = default);
}
```

**منطق الأعمال لـ ProductUnitService:**
- `AddUnitToProductAsync`: التحقق من أن الوحدة غير مضافة مسبقاً للمنتج + ConversionFactor > 0
- `SetDefaultUnitAsync`: يجب أن يكون هناك وحدة افتراضية واحدة فقط → يُلغي IsDefault من الوحدات الأخرى للمنتج نفسه داخل Transaction

#### [NEW] Catalog/Interfaces/IProductBarCodeService.cs

```csharp
public interface IProductBarCodeService
{
    Task<ServiceResult<IReadOnlyList<ProductBarCodeResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<ServiceResult<ProductBarCodeResponseDto>> AddBarCodeAsync(Guid productId, CreateProductBarCodeDto dto, CancellationToken ct = default);
    Task<ServiceResult> RemoveBarCodeAsync(Guid barCodeId, CancellationToken ct = default);
}
```

**منطق الأعمال:**
- `AddBarCodeAsync`: التحقق من عدم تكرار الـ BarCode string عبر كامل المستودع (ليس فقط المنتج)

#### [NEW] Catalog/Interfaces/IProductImageService.cs

```csharp
public interface IProductImageService
{
    Task<ServiceResult<IReadOnlyList<ProductImageResponseDto>>> GetByProductAsync(Guid productId, CancellationToken ct = default);
    Task<ServiceResult<ProductImageResponseDto>> AddImageAsync(Guid productId, CreateProductImageDto dto, CancellationToken ct = default);
    Task<ServiceResult> RemoveImageAsync(Guid imageId, CancellationToken ct = default);
    Task<ServiceResult> SetDefaultImageAsync(Guid imageId, CancellationToken ct = default);
}
```

---

### 4. Branch — الفروع

#### [NEW] Branch/Interfaces/IBranchService.cs

```csharp
public interface IBranchService
{
    Task<ServiceResult<BranchResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<BranchResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<PagedResult<BranchResponseDto>>> GetPagedAsync(int page, int size, CancellationToken ct = default);
    Task<ServiceResult<BranchResponseDto>> CreateAsync(CreateBranchDto dto, CancellationToken ct = default);
    Task<ServiceResult<BranchResponseDto>> UpdateAsync(Guid id, UpdateBranchDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
```

**منطق الأعمال لـ BranchService:**
- `CreateAsync`: إنشاء Branch مع أرقام الهاتف (BranchPhones) داخل Transaction واحدة
- `UpdateAsync`: مقارنة قائمة أرقام الهاتف القديمة والجديدة وإضافة/حذف الفرق
- `DeleteAsync`: الحذف الناعم — التحقق من عدم وجود مستخدمين نشطين مرتبطين بالفرع

---

### 5. Tenant — المستأجر

#### [NEW] Tenant/Interfaces/ITenantService.cs

```csharp
public interface ITenantService
{
    Task<ServiceResult<TenantResponseDto>> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult<IReadOnlyList<TenantResponseDto>>> GetAllAsync(CancellationToken ct = default);
    Task<ServiceResult<TenantResponseDto>> CreateAsync(CreateTenantDto dto, CancellationToken ct = default);
    Task<ServiceResult<TenantResponseDto>> UpdateAsync(Guid id, UpdateTenantDto dto, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(Guid id, CancellationToken ct = default);
    Task<ServiceResult> ToggleActiveStatusAsync(Guid id, CancellationToken ct = default);
}
```

**ملاحظة مهمة**: Tenant هو الوحيد الذي لا يخضع لـ Multi-Tenant Global Filter (كما هو محدد في AppDbContext)، لذا خدمته تعمل على مستوى النظام الكامل (System-Level Service).

---

### 6. Auth — المصادقة

#### [NEW] Auth/Interfaces/IAuthService.cs

```csharp
public interface IAuthService
{
    Task<ServiceResult<AuthResponseDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<ServiceResult<AuthResponseDto>> RegisterAsync(RegisterDto dto, CancellationToken ct = default);
    Task<ServiceResult> LogoutAsync(string userId, CancellationToken ct = default);
    Task<ServiceResult<AuthResponseDto>> RefreshTokenAsync(string refreshToken, CancellationToken ct = default);
}
```

**منطق الأعمال لـ AuthService:**
- يستخدم `UserManager<ApplicationUser>` و `SignInManager<ApplicationUser>` من ASP.NET Core Identity
- بعد التحقق من بيانات الدخول، يُنشئ JWT Token يتضمن Claim باسم `TenantId` (مطلوب لـ CurrentTenantService)
- يسجل BranchId في الـ Claims إن كان للمستخدم فرع محدد

---

### 7. FileUpload — رفع الملفات

#### [NEW] FileUpload/Interfaces/IFileUploadService.cs

```csharp
public interface IFileUploadService
{
    Task<ServiceResult<string>> UploadAsync(IFormFile file, string subfolder, CancellationToken ct = default);
    Task<ServiceResult> DeleteAsync(string fileUrl, CancellationToken ct = default);
    bool IsValidImageExtension(string fileName);
    bool IsWithinSizeLimit(long fileSizeBytes, long maxSizeBytes = 5 * 1024 * 1024);
}
```

## القرارات التقنية التفصيلية

### Specification Pattern — كيفية التطبيق

سيتم إضافة `ISpecification<T>` و `SpecificationEvaluator` لطبقة DataAccess، ثم تمرير الـ Specification للريبوستري:

```csharp
// في DataAccess — Specifications/
public interface ISpecification<T>
{
    Expression<Func<T, bool>>? Criteria { get; }
    List<Expression<Func<T, object>>> Includes { get; }
    List<string> IncludeStrings { get; }
    Expression<Func<T, object>>? OrderBy { get; }
    bool IsDescending { get; }
    int? Take { get; }
    int? Skip { get; }
}

// في Services — Product Specification:
public class ProductWithDetailsSpec : BaseSpecification<Product>
{
    public ProductWithDetailsSpec(Guid productId) : base(p => p.Id == productId)
    {
        AddInclude(p => p.Category!);
        AddInclude(p => p.ProductUnits);
        AddInclude(p => p.ProductBarCodes);
        AddInclude(p => p.ProductImages);
    }
}
```

يُضاف `FirstOrDefaultAsync(ISpecification<T>)` و `GetPagedAsync(ISpecification<T>)` لـ IRepository.

### نظام أكواد الخطأ الموحد

```csharp
public static class ErrorCodes
{
    // عام
    public const string NotFound          = "NOT_FOUND";
    public const string Unauthorized      = "UNAUTHORIZED";
    public const string ValidationError   = "VALIDATION_ERROR";
    public const string ConcurrencyError  = "CONCURRENCY_ERROR";

    // Tenant
    public const string TenantNotFound    = "TENANT_NOT_FOUND";
    public const string TenantNameExists  = "TENANT_NAME_EXISTS";

    // Auth
    public const string InvalidCredentials = "INVALID_CREDENTIALS";
    public const string UserAlreadyExists  = "USER_ALREADY_EXISTS";

    // Branch
    public const string BranchNotFound    = "BRANCH_NOT_FOUND";
    public const string BranchNameExists  = "BRANCH_NAME_EXISTS";
    public const string BranchHasUsers    = "BRANCH_HAS_ACTIVE_USERS";

    // Catalog
    public const string CategoryNotFound       = "CATEGORY_NOT_FOUND";
    public const string CategoryHasProducts    = "CATEGORY_HAS_PRODUCTS";
    public const string CategoryCircularRef    = "CATEGORY_CIRCULAR_REFERENCE";
    public const string CategoryNameExists     = "CATEGORY_NAME_EXISTS";
    public const string ProductNotFound        = "PRODUCT_NOT_FOUND";
    public const string UnitNotFound           = "UNIT_NOT_FOUND";
    public const string UnitNameExists         = "UNIT_NAME_EXISTS";
    public const string ProductUnitNotFound    = "PRODUCT_UNIT_NOT_FOUND";
    public const string ProductUnitDuplicate   = "PRODUCT_UNIT_DUPLICATE";
    public const string BarCodeNotFound        = "BARCODE_NOT_FOUND";
    public const string BarCodeDuplicate       = "BARCODE_DUPLICATE";
    public const string ImageNotFound          = "IMAGE_NOT_FOUND";

    // File Upload
    public const string InvalidFileType   = "INVALID_FILE_TYPE";
    public const string FileTooLarge      = "FILE_TOO_LARGE";
    public const string UploadFailed      = "UPLOAD_FAILED";
}
```

### JWT — Access Token فقط

```csharp
// AuthService ينشئ JWT يتضمن:
// - sub (UserId)
// - TenantId
// - BranchId (إن وُجد)
// - role
// انتهاء الصلاحية: 8 ساعات (قابل للضبط من appsettings.json)
// لا يوجد Refresh Token في بيئة الاختبار
```

### FileUpload — التخزين المحلي

```
wwwroot/
└── uploads/
    ├── products/    (صور المنتجات)
    └── logos/       (شعارات المستأجرين)
```

يُعاد URL نسبي: `/uploads/products/{tenantId}/{fileName}`

---

### 8. التسجيل في DI

#### [NEW] Extensions/ServicesLayerExtensions.cs

```csharp
public static class ServicesLayerExtensions
{
    public static IServiceCollection AddServicesLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(CatalogMappingProfile).Assembly);

        // Catalog Services
        services.AddScoped<ICategoryService, CategoryService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IUnitService, UnitService>();
        services.AddScoped<IProductUnitService, ProductUnitService>();
        services.AddScoped<IProductBarCodeService, ProductBarCodeService>();
        services.AddScoped<IProductImageService, ProductImageService>();

        // Branch Services
        services.AddScoped<IBranchService, BranchService>();

        // Tenant Services
        services.AddScoped<ITenantService, TenantService>();

        // Auth Services
        services.AddScoped<IAuthService, AuthService>();

        // File Upload
        services.AddScoped<IFileUploadService, FileUploadService>();

        return services;
    }
}
```

---

## مخطط تدفق البيانات

```
[HTTP Request]
      ↓
[Controller]
      ↓  (DTO)
[Service Layer]  ←── [IMapper (AutoMapper)]
      ↓
[IUnitOfWork]
      ↓
[IRepository<T>] → [AppDbContext] → [SQL Server]
      ↑
[Global Filters: SoftDelete + MultiTenancy]
```

---

## أولويات التنفيذ

| المرحلة | الخدمة | الأولوية | السبب |
|---------|--------|----------|-------|
| 1 | `TenantService` | 🔴 حرجة | مطلوبة لتسجيل المستأجرين الأوائل |
| 2 | `AuthService` | 🔴 حرجة | مطلوبة لكل العمليات الأخرى (JWT + Claims) |
| 3 | `BranchService` | 🟠 عالية | مطلوبة لربط المستخدمين بالفروع |
| 4 | `UnitService` | 🟠 عالية | أساسية قبل ProductUnitService |
| 5 | `CategoryService` | 🟡 متوسطة | أساسية قبل ProductService |
| 6 | `ProductService` | 🟡 متوسطة | محور الكتالوج |
| 7 | `ProductUnitService` | 🟢 عادية | امتداد للمنتج |
| 8 | `ProductBarCodeService` | 🟢 عادية | امتداد للمنتج |
| 9 | `ProductImageService` | 🟢 عادية | امتداد للمنتج |
| 10 | `FileUploadService` | 🟢 عادية | مساعدة لـ ProductImageService |

---

## Open Questions — أسئلة تحتاج قرار قبل التنفيذ

> [!IMPORTANT]
> **سياسة JWT**: هل يتم تخزين Refresh Token في قاعدة البيانات؟ أم في Redis؟ أم يُكتفى بـ Access Token فقط؟

> [!IMPORTANT]
> **رفع الملفات**: هل يُخزن في السيرفر المحلي (wwwroot)؟ أم في Azure Blob Storage؟ أم في AWS S3؟

> [!WARNING]
> **ProductService وعلاقاته**: `IRepository<Product>` لا يدعم Include مباشرة. هل نستخدم:
> - (أ) Include عبر IQueryable مباشرة على `_context` في الريبوستري (يكسر المجردة)
> - (ب) إضافة `GetWithIncludesAsync` لـ IRepository
> - (ج) إنشاء Specification Pattern لتعريف الـ Includes

> [!NOTE]
> **Pagination**: هل يتم توحيد معاملات الترقيم (PageNumber, PageSize) عبر Query String موحد؟ (مثلاً `?page=1&size=20`)

> [!NOTE]
> **ErrorCodes**: هل نريد نظام أكواد خطأ موحد (مثل `"PRODUCT_NOT_FOUND"`, `"BARCODE_DUPLICATE"`) أم رسائل نصية فقط؟

---

## خطة التحقق

### اختبارات تلقائية
```bash
dotnet build RetalSystemAPI.Services
dotnet test  # بعد إضافة مشروع Tests
```

### تحقق يدوي
1. استدعاء `POST /api/tenants` — إنشاء مستأجر
2. استدعاء `POST /api/auth/register` — تسجيل مستخدم وربطه بالمستأجر
3. استدعاء `POST /api/auth/login` — التحقق من وجود `TenantId` في JWT
4. استدعاء `GET /api/categories` — التحقق من فلترة Tenant التلقائية
5. استدعاء `POST /api/products` — إنشاء منتج مع فئة والتحقق من الـ AutoMapper
