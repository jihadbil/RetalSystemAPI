# مهام بناء طبقة الخدمات — RetalSystemAPI.Services

---

## المرحلة 0 — تهيئة المشروع وملف csproj

- `[ ]` **0.1** تعديل `RetalSystemAPI.Services.csproj` لإضافة مراجع المشاريع:
  - `ProjectReference` لـ `RetalSystemAPI.DataAccess`
  - `ProjectReference` لـ `RetalSystemAPI.Models.DTOs`
- `[ ]` **0.2** إضافة حزمة `AutoMapper` و`Microsoft.Extensions.DependencyInjection.Abstractions` و`Microsoft.AspNetCore.Identity.EntityFrameworkCore` عبر PackageReference
- `[ ]` **0.3** التحقق من نجاح `dotnet build` بعد إضافة المراجع

---

## المرحلة 1 — البنية التحتية المشتركة (Common Infrastructure)

> **ملاحظة**: يجب إنجاز هذه المرحلة بالكامل قبل الشروع في أي خدمة.

### 1-A: Specification Pattern في DataAccess

- `[ ]` **1.1** إنشاء `RetalSystemAPI.DataAccess/Specifications/ISpecification.cs`:
  - خاصية `Criteria`: شرط التصفية `Expression<Func<T, bool>>?`
  - خاصية `Includes`: قائمة Lambda Includes `List<Expression<Func<T, object>>>`
  - خاصية `IncludeStrings`: قائمة String Includes `List<string>`
  - خاصية `OrderBy`: ترتيب `Expression<Func<T, object>>?`
  - خاصية `IsDescending`: اتجاه الترتيب
  - خواص `Skip` و `Take` للـ Pagination
- `[ ]` **1.2** إنشاء `RetalSystemAPI.DataAccess/Specifications/BaseSpecification.cs`:
  - تنفيذ `ISpecification<T>`
  - دالة محمية `AddInclude(Expression)` لإضافة Lambda Include
  - دالة محمية `AddInclude(string)` لإضافة String Include
  - دالة `ApplyOrderBy` و `ApplyOrderByDescending`
  - دالة `ApplyPaging(skip, take)`
- `[ ]` **1.3** إنشاء `RetalSystemAPI.DataAccess/Specifications/SpecificationEvaluator.cs`:
  - دالة `static` تأخذ `IQueryable<T>` و `ISpecification<T>`
  - تطبيق Criteria → Includes → OrderBy → Skip/Take
- `[ ]` **1.4** تعديل `IRepository<T>` لإضافة overload بالـ Specification:
  - `Task<T?> FirstOrDefaultAsync(ISpecification<T> spec, CancellationToken ct = default)`
  - `Task<IReadOnlyList<T>> FindAsync(ISpecification<T> spec, CancellationToken ct = default)`
  - `Task<(IReadOnlyList<T> Items, int TotalCount)> GetPagedAsync(ISpecification<T> spec, int pageNumber, int pageSize, CancellationToken ct = default)`
- `[ ]` **1.5** تعديل `Repository<T>` لتنفيذ الـ overloads الجديدة باستخدام `SpecificationEvaluator`
- `[ ]` **1.6** `dotnet build` للتحقق من الـ Specification Layer

### 1-B: Common Models في Services

- `[ ]` **1.7** إنشاء `RetalSystemAPI.Services/Common/Models/ServiceResult.cs`:
  - كلاس `ServiceResult<T>` مع `IsSuccess`, `Data`, `ErrorMessage`, `ErrorCode`
  - Factory methods: `Success(T data)` و `Failure(string message, string? code)`
  - كلاس `ServiceResult` (بدون بيانات) لعمليات Void مثل الحذف
- `[ ]` **1.8** إنشاء `RetalSystemAPI.Services/Common/Models/PagedResult.cs`:
  - خواص: `Items`, `TotalCount`, `PageNumber`, `PageSize`
  - خواص محسوبة: `TotalPages`, `HasPreviousPage`, `HasNextPage`
  - دالة `static` `Create(items, totalCount, page, size)`
- `[ ]` **1.9** إنشاء `RetalSystemAPI.Services/Common/Models/ErrorCodes.cs`:
  - ثوابت `const string` لجميع أكواد الخطأ:
    - عامة: `NOT_FOUND`, `UNAUTHORIZED`, `VALIDATION_ERROR`, `CONCURRENCY_ERROR`
    - Tenant: `TENANT_NOT_FOUND`, `TENANT_NAME_EXISTS`
    - Auth: `INVALID_CREDENTIALS`, `USER_ALREADY_EXISTS`
    - Branch: `BRANCH_NOT_FOUND`, `BRANCH_NAME_EXISTS`, `BRANCH_HAS_ACTIVE_USERS`
    - Category: `CATEGORY_NOT_FOUND`, `CATEGORY_HAS_PRODUCTS`, `CATEGORY_CIRCULAR_REFERENCE`, `CATEGORY_NAME_EXISTS`
    - Product: `PRODUCT_NOT_FOUND`
    - Unit: `UNIT_NOT_FOUND`, `UNIT_NAME_EXISTS`
    - ProductUnit: `PRODUCT_UNIT_NOT_FOUND`, `PRODUCT_UNIT_DUPLICATE`
    - BarCode: `BARCODE_NOT_FOUND`, `BARCODE_DUPLICATE`
    - Image: `IMAGE_NOT_FOUND`
    - FileUpload: `INVALID_FILE_TYPE`, `FILE_TOO_LARGE`, `UPLOAD_FAILED`
- `[ ]` **1.10** `dotnet build` للتحقق من Common Layer

---

## المرحلة 2 — TenantService (أعلى أولوية)

> TenantService لا تخضع لـ Multi-Tenant Filter → تعمل على مستوى النظام الكامل.

- `[ ]` **2.1** إنشاء `Tenant/Interfaces/ITenantService.cs`:
  - `GetByIdAsync(Guid id)`
  - `GetAllAsync()`
  - `GetPagedAsync(int page, int size)`
  - `CreateAsync(CreateTenantDto dto)`
  - `UpdateAsync(Guid id, UpdateTenantDto dto)`
  - `DeleteAsync(Guid id)`
  - `ToggleActiveStatusAsync(Guid id)`
- `[ ]` **2.2** إنشاء `Tenant/Implementations/TenantService.cs`:
  - حقن `IUnitOfWork` و `IMapper`
  - **CreateAsync**: التحقق من تكرار `Name` → `ExistsAsync` ← إرجاع `TENANT_NAME_EXISTS`
  - **GetByIdAsync**: `GetByIdAsync` + تحقق null → `TENANT_NOT_FOUND`
  - **UpdateAsync**: جلب الكيان → تحديث → `Update` → `SaveChangesAsync`
  - **DeleteAsync**: جلب → `SoftDelete` → `SaveChangesAsync`
  - **ToggleActiveStatusAsync**: جلب → عكس `IsActive` → حفظ
  - **GetPagedAsync**: `GetPagedAsync` → تحويل لـ `PagedResult<TenantResponseDto>`
  - استخدام `ServiceResult` / `ServiceResult<T>` في جميع العمليات
- `[ ]` **2.3** `dotnet build` للتحقق

---

## المرحلة 3 — AuthService

> تعتمد على `UserManager<ApplicationUser>` و `SignInManager`. JWT يتضمن `TenantId` Claim المطلوب لـ CurrentTenantService.

- `[ ]` **3.1** إنشاء `Auth/Interfaces/IAuthService.cs`:
  - `LoginAsync(LoginDto dto)`
  - `RegisterAsync(RegisterDto dto)`
  - `LogoutAsync(string userId)` (رمزي في بيئة Access Token فقط)
- `[ ]` **3.2** إنشاء `Auth/Implementations/AuthService.cs`:
  - حقن `UserManager<ApplicationUser>`, `IConfiguration`, `IUnitOfWork`
  - **RegisterAsync**:
    - التحقق من عدم تكرار البريد الإلكتروني → `USER_ALREADY_EXISTS`
    - التحقق من وجود `TenantId` المُرسل → `TENANT_NOT_FOUND`
    - إنشاء مستخدم عبر `UserManager.CreateAsync`
    - إرجاع `AuthResponseDto` مع JWT
  - **LoginAsync**:
    - `FindByEmailAsync` → تحقق → `CheckPasswordAsync`
    - إرجاع `INVALID_CREDENTIALS` إن فشل
    - توليد JWT يتضمن: `sub`, `TenantId`, `BranchId`, `role`
    - صلاحية من `appsettings.json` (افتراضي 8 ساعات)
  - **GenerateJwtToken (private)**: دالة مساعدة لبناء JWT بـ Claims
- `[ ]` **3.3** `dotnet build` للتحقق

---

## المرحلة 4 — BranchService

> إنشاء Branch مع BranchPhones في Transaction واحدة.

- `[ ]` **4.1** إنشاء Specification للفروع:
  - `BranchWithPhonesSpec`: تضمين `BranchPhones`
  - `BranchWithPhonesAndUsersSpec`: تضمين `BranchPhones` + `ApplicationUser`
- `[ ]` **4.2** إنشاء `Branch/Interfaces/IBranchService.cs`:
  - `GetByIdAsync(Guid id)`
  - `GetAllAsync()`
  - `GetPagedAsync(int page, int size)`
  - `CreateAsync(CreateBranchDto dto)`
  - `UpdateAsync(Guid id, UpdateBranchDto dto)`
  - `DeleteAsync(Guid id)`
  - `ToggleActiveStatusAsync(Guid id)`
- `[ ]` **4.3** إنشاء `Branch/Implementations/BranchService.cs`:
  - **CreateAsync**:
    - التحقق من تكرار `Name` في نفس المستأجر → `BRANCH_NAME_EXISTS`
    - `AddAsync(branch)` ثم AddRange للـ BranchPhones (إن وُجدت)
    - `SaveChangesAsync()`
  - **UpdateAsync**:
    - جلب الفرع مع BranchPhones (Specification)
    - مقارنة قائمة الهواتف القديمة والجديدة
    - `HardDelete` للهواتف المحذوفة + `AddRangeAsync` للجديدة
    - تحديث حقول الفرع + `SaveChangesAsync`
  - **DeleteAsync**:
    - جلب الفرع مع Users (Specification)
    - رفض الحذف إن كان هناك مستخدمون نشطون → `BRANCH_HAS_ACTIVE_USERS`
    - `SoftDelete` + `SaveChangesAsync`
  - **GetPagedAsync**: Specification + Pagination + Map
- `[ ]` **4.4** `dotnet build` للتحقق

---

## المرحلة 5 — UnitService

> بسيطة نسبياً، لكنها مطلوبة قبل ProductUnitService.

- `[ ]` **5.1** إنشاء `Catalog/Interfaces/IUnitService.cs`:
  - `GetByIdAsync(Guid id)`
  - `GetAllAsync()`
  - `CreateAsync(CreateUnitDto dto)`
  - `UpdateAsync(Guid id, UpdateUnitDto dto)`
  - `DeleteAsync(Guid id)`
- `[ ]` **5.2** إنشاء `Catalog/Implementations/UnitService.cs`:
  - **CreateAsync**: التحقق من تكرار `Name` → `UNIT_NAME_EXISTS`
  - **DeleteAsync**: التحقق من عدم استخدام الوحدة في أي `ProductUnit` نشط → رسالة مخصصة
  - باقي العمليات: CRUD مباشر
- `[ ]` **5.3** `dotnet build` للتحقق

---

## المرحلة 6 — CategoryService

> يدعم هيكل هرمي (ParentCategoryId) — يجب التحقق من الحلقات الدائرية.

- `[ ]` **6.1** إنشاء Specifications للتصنيفات:
  - `CategoryWithSubCategoriesSpec(Guid id)`: تضمين `SubCategories` + `Products`
  - `RootCategoriesSpec`: فلترة `ParentCategoryId == null`
  - `SubCategoriesSpec(Guid parentId)`: فلترة بـ `ParentCategoryId`
- `[ ]` **6.2** إنشاء `Catalog/Interfaces/ICategoryService.cs`:
  - `GetByIdAsync(Guid id)`
  - `GetAllAsync()`
  - `GetRootCategoriesAsync()`
  - `GetSubCategoriesAsync(Guid parentId)`
  - `GetPagedAsync(int page, int size)`
  - `CreateAsync(CreateCategoryDto dto)`
  - `UpdateAsync(Guid id, UpdateCategoryDto dto)`
  - `DeleteAsync(Guid id)`
  - `ToggleActiveStatusAsync(Guid id)`
- `[ ]` **6.3** إنشاء `Catalog/Implementations/CategoryService.cs`:
  - **CreateAsync**:
    - التحقق من تكرار `Name` → `CATEGORY_NAME_EXISTS`
    - إن مُرر `ParentCategoryId`: التحقق من وجوده → `CATEGORY_NOT_FOUND`
  - **UpdateAsync**:
    - منع الحلقة الدائرية: التحقق من أن الـ `ParentCategoryId` الجديد ليس أحد أبناء هذا التصنيف أو هو نفسه → `CATEGORY_CIRCULAR_REFERENCE`
  - **DeleteAsync**:
    - رفض الحذف إن كان هناك منتجات أو تصنيفات فرعية نشطة → `CATEGORY_HAS_PRODUCTS`
    - `SoftDelete`
- `[ ]` **6.4** تنفيذ دالة مساعدة خاصة `IsCircularReference(Guid categoryId, Guid newParentId)`:
  - تصعد في الشجرة الهرمية بشكل تكراري للتحقق من وجود حلقة
- `[ ]` **6.5** `dotnet build` للتحقق

---

## المرحلة 7 — ProductService وامتداداته

> أكبر خدمة في النظام — تعتمد على Category + Unit + Specification Pattern.

### 7-A: Specifications للمنتجات

- `[ ]` **7.1** إنشاء `ProductWithDetailsSpec(Guid productId)`:
  - Includes: `Category`, `ProductUnits → Unit`, `ProductBarCodes → ProductImages`, `ProductImages`
- `[ ]` **7.2** إنشاء `ProductSummarySpec(int page, int size, Guid? categoryId)`:
  - Includes: `Category`, `ProductImages` (أول صورة فقط), `ProductBarCodes` (أول باركود)
  - تطبيق Filter بـ `categoryId` إن مُرر

### 7-B: ProductService

- `[ ]` **7.3** إنشاء `Catalog/Interfaces/IProductService.cs`:
  - `GetByIdAsync(Guid id)` → `ProductResponseDto`
  - `GetPagedAsync(int page, int size, Guid? categoryId)` → `PagedResult<ProductSummaryDto>`
  - `SearchAsync(string query)` → `IReadOnlyList<ProductSummaryDto>`
  - `GetByBarCodeAsync(string barCode)` → `ProductResponseDto`
  - `CreateAsync(CreateProductDto dto)` → `ProductResponseDto`
  - `UpdateAsync(Guid id, UpdateProductDto dto)` → `ProductResponseDto`
  - `DeleteAsync(Guid id)`
- `[ ]` **7.4** إنشاء `Catalog/Implementations/ProductService.cs`:
  - **CreateAsync**:
    - التحقق من وجود `CategoryId` → `CATEGORY_NOT_FOUND`
    - التحقق أن `CostPrice >= 0` و `SalePrice >= 0`
    - `AddAsync` + `SaveChangesAsync`
  - **UpdateAsync**:
    - جلب المنتج بـ `GetByIdAsync`
    - تحديث الحقول + `Update` + `SaveChangesAsync`
    - معالجة `DbUpdateConcurrencyException` ← `CONCURRENCY_ERROR`
  - **GetByIdAsync**: Specification + Map لـ `ProductResponseDto`
  - **GetByBarCodeAsync**: `FindAsync` على `ProductBarCodes` ← جلب المنتج بـ Specification
  - **SearchAsync**: فلترة بـ `Name.Contains(query)` أو `BarCode`
  - **DeleteAsync**: `SoftDelete` للمنتج (الكيانات الفرعية تُحذف تلقائياً بالـ Cascade أو بنفس الآلية)

### 7-C: ProductUnitService

- `[ ]` **7.5** إنشاء `Catalog/Interfaces/IProductUnitService.cs`:
  - `GetByProductAsync(Guid productId)`
  - `AddUnitToProductAsync(Guid productId, CreateProductUnitDto dto)`
  - `RemoveUnitFromProductAsync(Guid productUnitId)`
  - `SetDefaultUnitAsync(Guid productUnitId)`
- `[ ]` **7.6** إنشاء `Catalog/Implementations/ProductUnitService.cs`:
  - **AddUnitToProductAsync**:
    - التحقق من وجود `Product` → `PRODUCT_NOT_FOUND`
    - التحقق من وجود `Unit` → `UNIT_NOT_FOUND`
    - التحقق من عدم التكرار → `PRODUCT_UNIT_DUPLICATE`
    - التحقق أن `ConversionFactor > 0`
    - `AddAsync` + `SaveChangesAsync`
  - **SetDefaultUnitAsync**:
    - داخل `BeginTransactionAsync`:
      1. جلب جميع وحدات المنتج + تعيين `IsDefault = false`
      2. تعيين `IsDefault = true` للوحدة المحددة
      3. `CommitTransactionAsync`

### 7-D: ProductBarCodeService

- `[ ]` **7.7** إنشاء `Catalog/Interfaces/IProductBarCodeService.cs`:
  - `GetByProductAsync(Guid productId)`
  - `AddBarCodeAsync(Guid productId, CreateProductBarCodeDto dto)`
  - `RemoveBarCodeAsync(Guid barCodeId)`
- `[ ]` **7.8** إنشاء `Catalog/Implementations/ProductBarCodeService.cs`:
  - **AddBarCodeAsync**:
    - التحقق من وجود المنتج → `PRODUCT_NOT_FOUND`
    - التحقق من عدم تكرار `BarCode` (في كامل المستودع، ليس للمنتج فقط) → `BARCODE_DUPLICATE`
    - `AddAsync` + `SaveChangesAsync`
  - **RemoveBarCodeAsync**:
    - جلب BarCode → `BARCODE_NOT_FOUND`
    - `HardDelete` (BarCodes لا تُحذف ناعماً عادةً) + `SaveChangesAsync`

### 7-E: ProductImageService

- `[ ]` **7.9** إنشاء `Catalog/Interfaces/IProductImageService.cs`:
  - `GetByProductAsync(Guid productId)`
  - `AddImageAsync(Guid productId, CreateProductImageDto dto)`
  - `RemoveImageAsync(Guid imageId)`
  - `SetDefaultImageAsync(Guid imageId)`
- `[ ]` **7.10** إنشاء `Catalog/Implementations/ProductImageService.cs`:
  - **AddImageAsync**: التحقق من المنتج → إضافة الصورة
  - **SetDefaultImageAsync**:
    - داخل Transaction: إلغاء IsDefault من كل صور المنتج → تعيين IsDefault للصورة المحددة
  - **RemoveImageAsync**:
    - إن كانت الصورة افتراضية وهناك صور أخرى → تعيين أول صورة كافتراضية
    - `HardDelete` + استدعاء `IFileUploadService.DeleteAsync` لحذف الملف الفعلي

---

## المرحلة 8 — FileUploadService

> خدمة مساعدة — تُستخدم من داخل `ProductImageService`.

- `[ ]` **8.1** إنشاء `FileUpload/Interfaces/IFileUploadService.cs`:
  - `UploadAsync(IFormFile file, string subfolder)` → `ServiceResult<string>` (URL النسبي)
  - `DeleteAsync(string fileUrl)` → `ServiceResult`
  - `IsValidImageExtension(string fileName)` → `bool`
  - `IsWithinSizeLimit(long fileSizeBytes, long maxSizeBytes = 5MB)` → `bool`
- `[ ]` **8.2** إنشاء `FileUpload/Implementations/FileUploadService.cs`:
  - حقن `IWebHostEnvironment` للحصول على `wwwroot` path
  - **UploadAsync**:
    - التحقق من الامتداد (`.jpg`, `.jpeg`, `.png`, `.webp`) → `INVALID_FILE_TYPE`
    - التحقق من الحجم (≤ 5MB) → `FILE_TOO_LARGE`
    - توليد اسم ملف فريد: `{Guid}.{extension}`
    - المسار: `wwwroot/uploads/{subfolder}/{tenantId}/{fileName}`
    - إنشاء المجلد إن لم يوجد + حفظ الملف
    - إرجاع URL: `/uploads/{subfolder}/{tenantId}/{fileName}`
  - **DeleteAsync**:
    - تحويل URL النسبي إلى مسار مطلق
    - `File.Delete` مع التحقق من وجود الملف

---

## المرحلة 9 — التسجيل في DI والإعداد النهائي

- `[ ]` **9.1** إنشاء `Extensions/ServicesLayerExtensions.cs`:
  - تسجيل جميع الخدمات بـ `AddScoped`
  - تسجيل `AddAutoMapper` مع تجميع `CatalogMappingProfile`
- `[ ]` **9.2** تعديل `Program.cs` في المشروع الرئيسي:
  - استدعاء `services.AddServicesLayer(configuration)`
  - التأكد من أن `AddDataAccess` يُستدعى قبله
- `[ ]` **9.3** إضافة إعدادات JWT في `appsettings.json`:
  ```json
  "JwtSettings": {
    "Secret": "...",
    "ExpiryHours": 8,
    "Issuer": "RetalSystemAPI",
    "Audience": "RetalSystemAPI-Client"
  }
  ```
- `[ ]` **9.4** إضافة إعدادات رفع الملفات:
  ```json
  "FileUpload": {
    "MaxSizeMB": 5,
    "AllowedExtensions": [".jpg", ".jpeg", ".png", ".webp"]
  }
  ```
- `[ ]` **9.5** `dotnet build` للمشروع بالكامل — التأكد من عدم وجود أخطاء

---

## تتبع التقدم

| المرحلة | الوصف | الحالة |
|---------|-------|--------|
| 0 | تهيئة csproj | `[ ]` |
| 1-A | Specification Pattern | `[ ]` |
| 1-B | Common Models + ErrorCodes | `[ ]` |
| 2 | TenantService | `[ ]` |
| 3 | AuthService | `[ ]` |
| 4 | BranchService | `[ ]` |
| 5 | UnitService | `[ ]` |
| 6 | CategoryService | `[ ]` |
| 7-A/B | Product Specs + ProductService | `[ ]` |
| 7-C | ProductUnitService | `[ ]` |
| 7-D | ProductBarCodeService | `[ ]` |
| 7-E | ProductImageService | `[ ]` |
| 8 | FileUploadService | `[ ]` |
| 9 | DI Registration + Config | `[ ]` |
