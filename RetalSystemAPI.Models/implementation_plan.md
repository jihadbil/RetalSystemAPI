# خطة بناء طبقة نواقل البيانات (DTOs Layer)

## الملخص التنفيذي

بناءً على التحليل الشامل لطبقة `RetalSystemAPI.Models`، هذه الخطة تصف كيفية بناء طبقة DTOs
في مشروع `RetalSystemAPI.Models.DTOs` الفارغ حالياً، مع AutoMapper للتحويل التلقائي بين النماذج و DTOs.

---

## القرارات النهائية ✅

| القرار | الاختيار |
|--------|----------|
| آلية التحويل بين النماذج و DTOs | ✅ **AutoMapper** (تلقائي) |
| آلية التحقق من الصحة | ✅ **DataAnnotations** |
| النطاق | ✅ **النماذج الموجودة فقط** (Customers, Sales, Orders, Warehouse, Suppliers تُترك كما هي) |

---

## تحليل طبقة النماذج الحالية

### النماذج الموجودة والمستهدفة بـ DTOs

| الوحدة | النماذج | DTOs المطلوبة |
|--------|---------|--------------|
| **Common** | `BaseEntity`, `ValidationConstants` | `BaseDto` (مشترك) |
| **Root** | `ApplicationUser`, `Tenant` | Auth DTOs + Tenant DTOs |
| **Branchs** | `Branch`, `BranchPhone` | Branch DTOs |
| **Catalog** | `Category`, `Product`, `ProductBarCode`, `ProductImage`, `ProductUnit`, `Unit` | Catalog DTOs كاملة |
| **Enums** | `InvoiceStatus`, `PaymentStatus`, `SalesOrderStatus`, `WarehouseType` | تُستخدم مباشرةً في DTOs |

### الوحدات المستثناة (لا تغيير)
`Customers` / `Suppliers` / `Warehouse` / `Sales` / `Orders` — فارغة وتُترك كما هي.

---

## بنية النموذج الأساسي (للمرجع)

```
BaseEntity
├── Id: Guid
├── TenantId: Guid              ← للعزل متعدد المستأجرين
├── CreatedAt: DateTime
├── UpdatedAt: DateTime?
├── IsDeleted: bool             ← Soft Delete (لا يُرسل للعميل)
├── CreatedByUserId: string?
├── UpdatedByUserId: string?
└── RowVersion: byte[]          ← Optimistic Concurrency (لا يُرسل للعميل)
```

---

## هيكل المجلدات النهائي

```
RetalSystemAPI.Models.DTOs/
├── RetalSystemAPI.Models.DTOs.csproj   ← إضافة مراجع Models + AutoMapper
├── Common/
│   └── BaseDto.cs
├── Auth/
│   ├── LoginDto.cs
│   ├── RegisterDto.cs
│   └── AuthResponseDto.cs
├── Tenant/
│   ├── CreateTenantDto.cs
│   ├── UpdateTenantDto.cs
│   └── TenantResponseDto.cs
├── Branch/
│   ├── CreateBranchDto.cs
│   ├── UpdateBranchDto.cs
│   ├── BranchResponseDto.cs
│   ├── BranchPhoneDto.cs
│   └── BranchPhoneResponseDto.cs
├── Catalog/
│   ├── Category/
│   │   ├── CreateCategoryDto.cs
│   │   ├── UpdateCategoryDto.cs
│   │   └── CategoryResponseDto.cs
│   ├── Unit/
│   │   ├── CreateUnitDto.cs
│   │   ├── UpdateUnitDto.cs
│   │   └── UnitResponseDto.cs
│   ├── ProductBarCode/
│   │   ├── CreateProductBarCodeDto.cs
│   │   └── ProductBarCodeResponseDto.cs
│   ├── ProductImage/
│   │   ├── CreateProductImageDto.cs
│   │   └── ProductImageResponseDto.cs
│   ├── ProductUnit/
│   │   ├── CreateProductUnitDto.cs
│   │   └── ProductUnitResponseDto.cs
│   └── Product/
│       ├── CreateProductDto.cs
│       ├── UpdateProductDto.cs
│       ├── ProductSummaryDto.cs
│       └── ProductResponseDto.cs
└── MappingProfiles/
    ├── TenantMappingProfile.cs
    ├── BranchMappingProfile.cs
    └── CatalogMappingProfile.cs
```

---

## التغييرات المقترحة التفصيلية

### ─── 1. إعداد المشروع ───

#### [MODIFY] RetalSystemAPI.Models.DTOs.csproj
إضافة مرجع `RetalSystemAPI.Models` و `AutoMapper`:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AutoMapper" Version="13.0.1" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\RetalSystemAPI.Models\RetalSystemAPI.Models.csproj" />
  </ItemGroup>
</Project>
```

---

### ─── 2. Common ───

#### [NEW] Common/BaseDto.cs
```csharp
namespace RetalSystemAPI.Models.DTOs.Common;

/// <summary>
/// الناقل الأساسي المشترك — يحتوي فقط الحقول التي يحتاجها العميل.
/// لا يُكشف: IsDeleted, RowVersion, TenantId, CreatedByUserId
/// </summary>
public abstract class BaseDto
{
    public Guid Id          { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

### ─── 3. Auth DTOs ───

#### [NEW] Auth/LoginDto.cs
```csharp
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Auth;

public class LoginDto
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    public string UserName { get; set; } = null!;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    public string Password { get; set; } = null!;
}
```

#### [NEW] Auth/RegisterDto.cs
```csharp
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Auth;

public class RegisterDto
{
    [Required] public string UserName { get; set; } = null!;
    [Required][EmailAddress] public string Email { get; set; } = null!;
    [Required][MinLength(6)] public string Password { get; set; } = null!;
    [Required] public Guid TenantId { get; set; }
    [Required] public Guid BranchId { get; set; }
}
```

#### [NEW] Auth/AuthResponseDto.cs
```csharp
namespace RetalSystemAPI.Models.DTOs.Auth;

public class AuthResponseDto
{
    public string Token        { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime ExpiresAt  { get; set; }
    public string UserId       { get; set; } = null!;
    public string UserName     { get; set; } = null!;
    public Guid TenantId       { get; set; }
    public Guid BranchId       { get; set; }
}
```

---

### ─── 4. Tenant DTOs ───

#### [NEW] Tenant/CreateTenantDto.cs
```csharp
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.DTOs.Tenant;

public class CreateTenantDto
{
    [Required][MaxLength(200)] public string Name { get; set; } = null!;
    public string? Description { get; set; }
    [EmailAddress] public string? ContactEmail { get; set; }
    [Required]
    [RegularExpression(ValidationConstants.LibyanPhonePattern,
        ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
}
```

#### [NEW] Tenant/UpdateTenantDto.cs
```csharp
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.DTOs.Tenant;

public class UpdateTenantDto
{
    [Required] public Guid Id { get; set; }
    [Required][MaxLength(200)] public string Name { get; set; } = null!;
    public string? Description { get; set; }
    [EmailAddress] public string? ContactEmail { get; set; }
    [Required]
    [RegularExpression(ValidationConstants.LibyanPhonePattern,
        ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public string PhoneNumber { get; set; } = null!;
    public string Address { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}
```

#### [NEW] Tenant/TenantResponseDto.cs
```csharp
using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Tenant;

public class TenantResponseDto : BaseDto
{
    public string Name          { get; set; } = null!;
    public string? Description  { get; set; }
    public string? ContactEmail { get; set; }
    public string PhoneNumber   { get; set; } = null!;
    public string Address       { get; set; } = null!;
    public string LogoUrl       { get; set; } = null!;
    public bool IsActive        { get; set; }
}
```

---

### ─── 5. Branch DTOs ───

#### [NEW] Branch/BranchPhoneDto.cs (مضمّن في Create/Update)
```csharp
using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.DTOs.Branch;

public class BranchPhoneDto
{
    public string? Name { get; set; }
    [Required]
    [RegularExpression(ValidationConstants.LibyanPhonePattern,
        ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public string PhoneNumber { get; set; } = null!;
}
```

#### [NEW] Branch/BranchPhoneResponseDto.cs
```csharp
using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Branch;

public class BranchPhoneResponseDto : BaseDto
{
    public string? Name       { get; set; }
    public string PhoneNumber { get; set; } = null!;
}
```

#### [NEW] Branch/CreateBranchDto.cs
```csharp
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Branch;

public class CreateBranchDto
{
    [Required][MaxLength(200)] public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
    public List<BranchPhoneDto> Phones { get; set; } = new();
}
```

#### [NEW] Branch/UpdateBranchDto.cs
```csharp
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Branch;

public class UpdateBranchDto
{
    [Required] public Guid Id { get; set; }
    [Required][MaxLength(200)] public string Name { get; set; } = null!;
    public string? Address { get; set; }
    public bool IsActive { get; set; }
    public List<BranchPhoneDto> Phones { get; set; } = new();
}
```

#### [NEW] Branch/BranchResponseDto.cs
```csharp
using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Branch;

public class BranchResponseDto : BaseDto
{
    public string Name     { get; set; } = null!;
    public string? Address { get; set; }
    public bool IsActive   { get; set; }
    public List<BranchPhoneResponseDto> Phones { get; set; } = new();
}
```

---

### ─── 6. Catalog DTOs ───

#### [NEW] Catalog/Category/CreateCategoryDto.cs
```csharp
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.Category;

public class CreateCategoryDto
{
    [Required][MaxLength(200)] public string Name { get; set; } = null!;
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; } = 0;
}
```

#### [NEW] Catalog/Category/UpdateCategoryDto.cs
```csharp
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Catalog.Category;

public class UpdateCategoryDto
{
    [Required] public Guid Id { get; set; }
    [Required][MaxLength(200)] public string Name { get; set; } = null!;
    public Guid? ParentCategoryId { get; set; }
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
}
```

#### [NEW] Catalog/Category/CategoryResponseDto.cs
```csharp
using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Catalog.Category;

public class CategoryResponseDto : BaseDto
{
    public string Name                    { get; set; } = null!;
    public Guid? ParentCategoryId         { get; set; }
    public string? ParentCategoryName     { get; set; }
    public bool IsActive                  { get; set; }
    public int SortOrder                  { get; set; }
    public int ProductCount               { get; set; }
    public List<CategoryResponseDto> SubCategories { get; set; } = new();
}
```

#### [NEW] Catalog/Unit/CreateUnitDto.cs + UpdateUnitDto.cs + UnitResponseDto.cs
```csharp
// CreateUnitDto
public class CreateUnitDto
{
    [Required][MaxLength(100)] public string Name { get; set; } = null!;
    public string? Description { get; set; }
    [Range(1, int.MaxValue)] public int UnitPackage { get; set; } = 1;
}
// UpdateUnitDto
public class UpdateUnitDto
{
    [Required] public Guid Id { get; set; }
    [Required][MaxLength(100)] public string Name { get; set; } = null!;
    public string? Description { get; set; }
    [Range(1, int.MaxValue)] public int UnitPackage { get; set; } = 1;
}
// UnitResponseDto
public class UnitResponseDto : BaseDto
{
    public string Name        { get; set; } = null!;
    public string? Description { get; set; }
    public int UnitPackage    { get; set; }
}
```

#### [NEW] Catalog/ProductBarCode DTOs + ProductImage DTOs + ProductUnit DTOs
(موضحة في ملف المهام بالكامل)

#### [NEW] Catalog/Product/ProductSummaryDto.cs (للقوائم)
```csharp
public class ProductSummaryDto : BaseDto
{
    public string Name             { get; set; } = null!;
    public string CategoryName     { get; set; } = null!;
    public decimal SalePrice       { get; set; }
    public decimal CostPrice       { get; set; }
    public string? DefaultImage    { get; set; }
    public string? DefaultBarCode  { get; set; }
}
```

#### [NEW] Catalog/Product/ProductResponseDto.cs (للتفاصيل)
```csharp
public class ProductResponseDto : BaseDto
{
    public string Name              { get; set; } = null!;
    public string? Description      { get; set; }
    public decimal CostPrice        { get; set; }
    public decimal SalePrice        { get; set; }
    public decimal AveragePrice     { get; set; }
    public Guid CategoryId          { get; set; }
    public string CategoryName      { get; set; } = null!;
    public List<ProductUnitResponseDto> Units      { get; set; } = new();
    public List<ProductBarCodeResponseDto> BarCodes { get; set; } = new();
    public List<ProductImageResponseDto> Images    { get; set; } = new();
}
```

---

### ─── 7. AutoMapper Profiles ───

#### [NEW] MappingProfiles/TenantMappingProfile.cs
```csharp
using AutoMapper;
using RetalSystemAPI.Models;
using RetalSystemAPI.Models.DTOs.Tenant;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

public class TenantMappingProfile : Profile
{
    public TenantMappingProfile()
    {
        CreateMap<Tenant, TenantResponseDto>();
        CreateMap<CreateTenantDto, Tenant>();
        CreateMap<UpdateTenantDto, Tenant>()
            .ForMember(dest => dest.Id, opt => opt.Ignore()); // Id من الـ URL
    }
}
```

#### [NEW] MappingProfiles/BranchMappingProfile.cs
```csharp
using AutoMapper;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.DTOs.Branch;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

public class BranchMappingProfile : Profile
{
    public BranchMappingProfile()
    {
        CreateMap<Branch, BranchResponseDto>()
            .ForMember(dest => dest.Phones,
                opt => opt.MapFrom(src => src.BranchPhones));
        CreateMap<BranchPhone, BranchPhoneResponseDto>();
        CreateMap<CreateBranchDto, Branch>();
        CreateMap<UpdateBranchDto, Branch>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
```

#### [NEW] MappingProfiles/CatalogMappingProfile.cs
```csharp
using AutoMapper;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.DTOs.Catalog.*;

namespace RetalSystemAPI.Models.DTOs.MappingProfiles;

public class CatalogMappingProfile : Profile
{
    public CatalogMappingProfile()
    {
        // Category
        CreateMap<Category, CategoryResponseDto>()
            .ForMember(dest => dest.ParentCategoryName,
                opt => opt.MapFrom(src => src.ParentCategory != null
                    ? src.ParentCategory.Name : null))
            .ForMember(dest => dest.ProductCount,
                opt => opt.MapFrom(src => src.Products.Count));
        CreateMap<CreateCategoryDto, Category>();
        CreateMap<UpdateCategoryDto, Category>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // Unit
        CreateMap<Unit, UnitResponseDto>();
        CreateMap<CreateUnitDto, Unit>();
        CreateMap<UpdateUnitDto, Unit>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        // ProductBarCode
        CreateMap<ProductBarCode, ProductBarCodeResponseDto>();
        CreateMap<CreateProductBarCodeDto, ProductBarCode>();

        // ProductImage
        CreateMap<ProductImage, ProductImageResponseDto>();
        CreateMap<CreateProductImageDto, ProductImage>();

        // ProductUnit
        CreateMap<ProductUnit, ProductUnitResponseDto>()
            .ForMember(dest => dest.UnitName,
                opt => opt.MapFrom(src => src.Unit != null ? src.Unit.Name : null));
        CreateMap<CreateProductUnitDto, ProductUnit>();

        // Product
        CreateMap<Product, ProductResponseDto>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null));
        CreateMap<Product, ProductSummaryDto>()
            .ForMember(dest => dest.CategoryName,
                opt => opt.MapFrom(src => src.Category != null ? src.Category.Name : null))
            .ForMember(dest => dest.DefaultImage,
                opt => opt.MapFrom(src =>
                    src.ProductImages.FirstOrDefault(i => i.IsDefault) != null
                    ? src.ProductImages.First(i => i.IsDefault).ImageUrl : null))
            .ForMember(dest => dest.DefaultBarCode,
                opt => opt.MapFrom(src =>
                    src.ProductBarCodes.FirstOrDefault() != null
                    ? src.ProductBarCodes.First().BarCode : null));
        CreateMap<CreateProductDto, Product>();
        CreateMap<UpdateProductDto, Product>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());
    }
}
```

---

## خطة التحقق

### التحقق البنيوي
```bash
dotnet build RetalSystemAPI.Models.DTOs
```

### التحقق من تسجيل AutoMapper
```csharp
// في Program.cs — تسجيل جميع Profiles من مجمعة DTOs
builder.Services.AddAutoMapper(typeof(TenantMappingProfile).Assembly);
```

### التحقق من صحة التحويل
- اختبار يدوي: إنشاء وحدة `Unit` ثم تحويلها إلى `UnitResponseDto` والتأكد من تطابق الحقول
- التأكد من عدم circular reference في `CategoryResponseDto.SubCategories`
