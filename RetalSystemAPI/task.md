# ✅ مهام بناء طبقة المتحكمات — Controllers Layer

---

## المرحلة 1: البنية التحتية المشتركة

### 1.1 — إنشاء `ApiResponse.cs`
- `[x]` إنشاء مجلد `Responses/` داخل `RetalSystemAPI`
- `[x]` إنشاء الملف `Responses/ApiResponse.cs`
- `[x]` تعريف `ApiResponse<T>` مع خصائص: `Success`, `Data`, `Message`, `ErrorCode`
- `[x]` إضافة factory methods: `Ok(T data)` و `Fail(string message, string? code)`
- `[x]` تعريف `ApiResponse` (بدون Generic) لعمليات بلا بيانات
- `[x]` إضافة factory methods: `Ok()` و `Fail(string message, string? code)`

---

### 1.2 — إنشاء `BaseApiController.cs`
- `[x]` إنشاء مجلد `Controllers/Base/`
- `[x]` إنشاء الملف `Controllers/Base/BaseApiController.cs`
- `[x]` إضافة سمات `[ApiController]` و `[Route("api/[controller]")]`
- `[x]` وراثة `ControllerBase`
- `[x]` تنفيذ `ToActionResult<T>(ServiceResult<T> result)` → تحويل للـ HTTP
- `[x]` تنفيذ `ToActionResult(ServiceResult result)` → للعمليات بلا بيانات
- `[x]` تنفيذ `ResolveStatusCode(string? errorCode)` بجدول تعيين شامل:
  - `[x]` أكواد `*_NOT_FOUND` → 404
  - `[x]` أكواد `UNAUTHORIZED`, `INVALID_CREDENTIALS` → 401
  - `[x]` أكواد `*_EXISTS`, `*_DUPLICATE`, `CONCURRENCY_ERROR` → 409
  - `[x]` أكواد `*_HAS_*`, `CIRCULAR_REF`, `*_IN_USE` → 422
  - `[x]` أكواد `INVALID_FILE_TYPE`, `FILE_TOO_LARGE`, `VALIDATION_ERROR` → 400

---

## المرحلة 2: تعديل `Program.cs` — JWT المحلي

- `[x]` حذف `using Microsoft.AspNetCore.Authentication;`
- `[x]` حذف `using Microsoft.Identity.Web;`
- `[x]` إضافة `using Microsoft.AspNetCore.Authentication.JwtBearer;`
- `[x]` إضافة `using Microsoft.IdentityModel.Tokens;`
- `[x]` إضافة `using System.Text;`
- `[x]` استبدال `.AddMicrosoftIdentityWebApi(...)` بـ `.AddJwtBearer(options => { ... })`
- `[x]` ضبط `TokenValidationParameters`:
  - `[x]` `ValidateIssuer = true`
  - `[x]` `ValidateAudience = true`
  - `[x]` `ValidateLifetime = true`
  - `[x]` `ValidateIssuerSigningKey = true`
  - `[x]` ربط `ValidIssuer` من `JwtSettings:Issuer`
  - `[x]` ربط `ValidAudience` من `JwtSettings:Audience`
  - `[x]` ربط `IssuerSigningKey` من `JwtSettings:Secret`
- `[x]` إزالة مرجع حزمة `Microsoft.Identity.Web` من ملف `.csproj`

---

## المرحلة 3: `AuthController`

- `[x]` إنشاء مجلد `Controllers/Auth/`
- `[x]` إنشاء `Controllers/Auth/AuthController.cs`
- `[x]` الوراثة من `BaseApiController`
- `[x]` تعيين المسار `[Route("api/auth")]`
- `[x]` حقن `IAuthService` عبر Constructor
- `[x]` **POST** `login`:
  - `[x]` قبول `[FromBody] LoginDto`
  - `[x]` استدعاء `_authService.LoginAsync(dto, ct)`
  - `[x]` إرجاع `200 OK` عبر `ToActionResult`
- `[x]` **POST** `register`:
  - `[x]` قبول `[FromBody] RegisterDto`
  - `[x]` استدعاء `_authService.RegisterAsync(dto, ct)`
  - `[x]` إرجاع `201 Created` عند النجاح
- `[x]` **POST** `logout`:
  - `[x]` استخراج `userId` من `User.FindFirstValue(ClaimTypes.NameIdentifier)`
  - `[x]` استدعاء `_authService.LogoutAsync(userId, ct)`
  - `[x]` تطبيق `[Authorize]` فقط على `logout`
- `[x]` تطبيق `[AllowAnonymous]` على `login` و `register`

---

## المرحلة 4: `TenantsController`

- `[x]` إنشاء مجلد `Controllers/Tenants/`
- `[x]` إنشاء `Controllers/Tenants/TenantsController.cs`
- `[x]` الوراثة من `BaseApiController`
- `[x]` حقن `ITenantService` عبر Constructor
- `[x]` تطبيق `[Authorize]` على مستوى الكلاس
- `[x]` **GET** `/` → `GetAllAsync` → `200`
- `[x]` **GET** `/paged?page=1&size=10` → `GetPagedAsync` → `200`
- `[x]` **GET** `/{id:guid}` → `GetByIdAsync` → `200` أو `404`
- `[x]` **POST** `/` → `CreateAsync` → `201 Created`
- `[x]` **PUT** `/{id:guid}` → `UpdateAsync` → `200` أو `404/409`
- `[x]` **DELETE** `/{id:guid}` → `DeleteAsync` → `200` أو `404`
- `[x]` **PATCH** `/{id:guid}/toggle-active` → `ToggleActiveStatusAsync` → `200` أو `404`

---

## المرحلة 5: `BranchesController`

- `[x]` إنشاء مجلد `Controllers/Branches/`
- `[x]` إنشاء `Controllers/Branches/BranchesController.cs`
- `[x]` الوراثة من `BaseApiController`
- `[x]` حقن `IBranchService` عبر Constructor
- `[x]` تطبيق `[Authorize]` على مستوى الكلاس
- `[x]` **GET** `/` → `GetAllAsync` → `200`
- `[x]` **GET** `/paged?page=1&size=10` → `GetPagedAsync` → `200`
- `[x]` **GET** `/{id:guid}` → `GetByIdAsync` (مع الهواتف) → `200` أو `404`
- `[x]` **POST** `/` → `CreateAsync` → `201 Created`
- `[x]` **PUT** `/{id:guid}` → `UpdateAsync` → `200` أو `404/409`
- `[x]` **DELETE** `/{id:guid}` → `DeleteAsync` → `200` أو `404/422`
- `[x]` **PATCH** `/{id:guid}/toggle-active` → `ToggleActiveStatusAsync` → `200` أو `404`

---

## المرحلة 6: `CategoriesController`

- `[x]` إنشاء مجلد `Controllers/Catalog/`
- `[x]` إنشاء `Controllers/Catalog/CategoriesController.cs`
- `[x]` الوراثة من `BaseApiController`
- `[x]` تعيين المسار `[Route("api/catalog/categories")]`
- `[x]` حقن `ICategoryService` عبر Constructor
- `[x]` تطبيق `[Authorize]` على مستوى الكلاس
- `[x]` **GET** `/` → `GetAllAsync` → `200`
- `[x]` **GET** `/roots` → `GetRootCategoriesAsync` → `200`
- `[x]` **GET** `/{id:guid}/children` → `GetSubCategoriesAsync` → `200`
- `[x]` **GET** `/paged?page=1&size=10` → `GetPagedAsync` → `200`
- `[x]` **GET** `/{id:guid}` → `GetByIdAsync` → `200` أو `404`
- `[x]` **POST** `/` → `CreateAsync` → `201 Created`
- `[x]` **PUT** `/{id:guid}` → `UpdateAsync` → `200` أو `404/409/422`
- `[x]` **DELETE** `/{id:guid}` → `DeleteAsync` → `200` أو `404/422`
- `[x]` **PATCH** `/{id:guid}/toggle-active` → `ToggleActiveStatusAsync` → `200` أو `404`

---

## المرحلة 7: `UnitsController`

- `[x]` إنشاء `Controllers/Catalog/UnitsController.cs`
- `[x]` الوراثة من `BaseApiController`
- `[x]` تعيين المسار `[Route("api/catalog/units")]`
- `[x]` حقن `IUnitService` عبر Constructor
- `[x]` تطبيق `[Authorize]` على مستوى الكلاس
- `[x]` **GET** `/` → `GetAllAsync` → `200`
- `[x]` **GET** `/{id:guid}` → `GetByIdAsync` → `200` أو `404`
- `[x]` **POST** `/` → `CreateAsync` → `201 Created`
- `[x]` **PUT** `/{id:guid}` → `UpdateAsync` → `200` أو `404/409`
- `[x]` **DELETE** `/{id:guid}` → `DeleteAsync` → `200` أو `404/422`

---

## المرحلة 8: `ProductsController`

- `[x]` إنشاء `Controllers/Catalog/ProductsController.cs`
- `[x]` الوراثة من `BaseApiController`
- `[x]` تعيين المسار `[Route("api/catalog/products")]`
- `[x]` حقن `IProductService` عبر Constructor
- `[x]` تطبيق `[Authorize]` على مستوى الكلاس
- `[x]` **GET** `/paged?page=1&size=10&categoryId=guid` → `GetPagedAsync` → `200`
- `[x]` **GET** `/search?q=text` → `SearchAsync` → `200`
- `[x]` **GET** `/{id:guid}` → `GetByIdAsync` → `200` أو `404`
- `[x]` **GET** `/by-barcode/{barCode}` → `GetByBarCodeAsync` → `200` أو `404`
- `[x]` **POST** `/` → `CreateAsync` → `201 Created`
- `[x]` **PUT** `/{id:guid}` → `UpdateAsync` → `200` أو `404/409`
- `[x]` **DELETE** `/{id:guid}` → `DeleteAsync` → `200` أو `404`

---

## المرحلة 9: `ProductUnitsController`

- `[x]` إنشاء `Controllers/Catalog/ProductUnitsController.cs`
- `[x]` الوراثة من `BaseApiController`
- `[x]` تعيين المسار `[Route("api/catalog/products/{productId:guid}/units")]`
- `[x]` حقن `IProductUnitService` عبر Constructor
- `[x]` تطبيق `[Authorize]` على مستوى الكلاس
- `[x]` **GET** `/` → `GetByProductAsync(productId)` → `200`
- `[x]` **POST** `/` → `AddUnitToProductAsync(productId, dto)` → `201 Created`
- `[x]` **DELETE** `/{productUnitId:guid}` → `RemoveUnitFromProductAsync` → `200` أو `404`
- `[x]` **PATCH** `/{productUnitId:guid}/set-default` → `SetDefaultUnitAsync` → `200` أو `404`

---

## المرحلة 10: `ProductBarCodesController`

- `[x]` إنشاء `Controllers/Catalog/ProductBarCodesController.cs`
- `[x]` الوراثة من `BaseApiController`
- `[x]` تعيين المسار `[Route("api/catalog/products/{productId:guid}/barcodes")]`
- `[x]` حقن `IProductBarCodeService` عبر Constructor
- `[x]` تطبيق `[Authorize]` على مستوى الكلاس
- `[x]` **GET** `/` → `GetByProductAsync(productId)` → `200`
- `[x]` **POST** `/` → `AddBarCodeAsync(productId, dto)` → `201 Created`
- `[x]` **DELETE** `/{barCodeId:guid}` → `RemoveBarCodeAsync` → `200` أو `404`

---

## المرحلة 11: `ProductImagesController`

- `[x]` إنشاء `Controllers/Catalog/ProductImagesController.cs`
- `[x]` الوراثة من `BaseApiController`
- `[x]` تعيين المسار `[Route("api/catalog/products/{productId:guid}/images")]`
- `[x]` حقن `IProductImageService` و `IFileUploadService` عبر Constructor
- `[x]` تطبيق `[Authorize]` على مستوى الكلاس
- `[x]` **GET** `/` → `GetByProductAsync(productId)` → `200`
- `[x]` **POST** `/` (رفع صورة):
  - `[x]` قبول `IFormFile` من `[FromForm]`
  - `[x]` التحقق من امتداد الملف عبر `_fileUploadService.IsValidImageExtension`
  - `[x]` التحقق من حجم الملف عبر `_fileUploadService.IsWithinSizeLimit`
  - `[x]` رفع الملف عبر `_fileUploadService.UploadAsync(file, "products")`
  - `[x]` إنشاء `CreateProductImageDto` بـ `imageUrl` المُعادة
  - `[x]` استدعاء `_productImageService.AddImageAsync(productId, dto)` → `201 Created`
- `[x]` **DELETE** `/{imageId:guid}`:
  - `[x]` استدعاء `_productImageService.RemoveImageAsync(imageId)` → `200` أو `404`
- `[x]` **PATCH** `/{imageId:guid}/set-default` → `SetDefaultImageAsync` → `200` أو `404`

---

## المرحلة 12: التحقق النهائي

- `[x]` بناء المشروع بنجاح (`dotnet build`)
- `[x]` اختبار `POST /api/auth/register` → `201 + JWT`
- `[x]` اختبار `POST /api/auth/login` → `200 + JWT`
- `[x]` اختبار `POST /api/auth/login` ببيانات خاطئة → `401 + INVALID_CREDENTIALS`
- `[x]` اختبار `GET /api/tenants` بلا Token → `401`
- `[x]` اختبار `GET /api/tenants` بـ Bearer Token → `200`
- `[x]` اختبار `GET /api/tenants/{id-وهمي}` → `404 + TenantNotFound`
- `[x]` اختبار `POST /api/catalog/categories` → `201`
- `[x]` اختبار `DELETE /api/catalog/categories/{id-له-منتجات}` → `422 + CATEGORY_HAS_PRODUCTS`
- `[x]` اختبار `POST /api/catalog/products/{id}/images` برفع صورة → `201`
- `[x]` اختبار `POST /api/catalog/products/{id}/images` بملف غير مسموح → `400 + INVALID_FILE_TYPE`
