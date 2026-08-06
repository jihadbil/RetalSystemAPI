# مهام بناء طبقة نواقل البيانات (DTOs Layer)

## الحالة: ✅ مكتملة

---

## المرحلة الأولى — إعداد المشروع

- `[x]` **[1.1]** تعديل `RetalSystemAPI.Models.DTOs.csproj`
  - إضافة `<PackageReference Include="AutoMapper" Version="13.0.1" />`
  - إضافة `<ProjectReference>` إلى `RetalSystemAPI.Models.csproj`
- `[x]` **[1.2]** التأكد من نجاح `dotnet build` بعد التعديل

---

## المرحلة الثانية — Common

- `[x]` **[2.1]** إنشاء `Common/BaseDto.cs`
  - خصائص: `Id: Guid`, `CreatedAt: DateTime`, `UpdatedAt: DateTime?`

---

## المرحلة الثالثة — Auth DTOs

- `[x]` **[3.1]** إنشاء `Auth/LoginDto.cs`
- `[x]` **[3.2]** إنشاء `Auth/RegisterDto.cs`
- `[x]` **[3.3]** إنشاء `Auth/AuthResponseDto.cs`

---

## المرحلة الرابعة — Tenant DTOs

- `[x]` **[4.1]** إنشاء `Tenant/CreateTenantDto.cs`
- `[x]` **[4.2]** إنشاء `Tenant/UpdateTenantDto.cs`
- `[x]` **[4.3]** إنشاء `Tenant/TenantResponseDto.cs`

---

## المرحلة الخامسة — Branch DTOs

- `[x]` **[5.1]** إنشاء `Branch/BranchPhoneDto.cs`
- `[x]` **[5.2]** إنشاء `Branch/BranchPhoneResponseDto.cs`
- `[x]` **[5.3]** إنشاء `Branch/CreateBranchDto.cs`
- `[x]` **[5.4]** إنشاء `Branch/UpdateBranchDto.cs`
- `[x]` **[5.5]** إنشاء `Branch/BranchResponseDto.cs`

---

## المرحلة السادسة — Catalog DTOs

### Category
- `[x]` **[6.1]** إنشاء `Catalog/Category/CreateCategoryDto.cs`
- `[x]` **[6.2]** إنشاء `Catalog/Category/UpdateCategoryDto.cs`
- `[x]` **[6.3]** إنشاء `Catalog/Category/CategoryResponseDto.cs`

### Unit
- `[x]` **[6.4]** إنشاء `Catalog/Unit/CreateUnitDto.cs`
- `[x]` **[6.5]** إنشاء `Catalog/Unit/UpdateUnitDto.cs`
- `[x]` **[6.6]** إنشاء `Catalog/Unit/UnitResponseDto.cs`

### ProductBarCode
- `[x]` **[6.7]** إنشاء `Catalog/ProductBarCode/CreateProductBarCodeDto.cs`
- `[x]` **[6.8]** إنشاء `Catalog/ProductBarCode/ProductBarCodeResponseDto.cs`

### ProductImage
- `[x]` **[6.9]** إنشاء `Catalog/ProductImage/CreateProductImageDto.cs`
- `[x]` **[6.10]** إنشاء `Catalog/ProductImage/ProductImageResponseDto.cs`

### ProductUnit
- `[x]` **[6.11]** إنشاء `Catalog/ProductUnit/CreateProductUnitDto.cs`
- `[x]` **[6.12]** إنشاء `Catalog/ProductUnit/ProductUnitResponseDto.cs`

### Product
- `[x]` **[6.13]** إنشاء `Catalog/Product/CreateProductDto.cs`
- `[x]` **[6.14]** إنشاء `Catalog/Product/UpdateProductDto.cs`
- `[x]` **[6.15]** إنشاء `Catalog/Product/ProductSummaryDto.cs`
- `[x]` **[6.16]** إنشاء `Catalog/Product/ProductResponseDto.cs`

---

## المرحلة السابعة — AutoMapper Profiles

- `[x]` **[7.1]** إنشاء `MappingProfiles/TenantMappingProfile.cs`
- `[x]` **[7.2]** إنشاء `MappingProfiles/BranchMappingProfile.cs`
- `[x]` **[7.3]** إنشاء `MappingProfiles/CatalogMappingProfile.cs`

---

## المرحلة الثامنة — التحقق والبناء

- `[x]` **[8.1]** تشغيل `dotnet build` على `RetalSystemAPI.Models.DTOs`
- `[x]` **[8.2]** التثبت من صحة الربط مع AutoMapper
