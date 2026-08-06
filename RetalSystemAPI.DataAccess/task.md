# 📋 ملف المهام — بناء طبقة DataAccess

> **المشروع:** RetalSystemAPI — DataAccess Layer  
> **التقنية:** .NET 10 · EF Core 10 · SQL Server · Identity  
> **النمط:** Generic Repository + Unit of Work + Global Query Filters

---

## 🔧 المرحلة الأولى — إصلاح طبقة Models

### 1.1 — تحديث BaseEntity
- [x] فتح [BaseEntity.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Common/BaseEntity.cs)
- [x] إضافة `public Guid TenantId { get; set; }` للكيان الأساسي
- [x] إضافة `public Tenant? Tenant { get; set; }` للكيان الأساسي
- [x] التأكد من ترتيب الخصائص: Id → TenantId → Audit → IsDeleted → RowVersion

### 1.2 — إنشاء ValidationConstants
- [x] إنشاء ملف [ValidationConstants.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Common/ValidationConstants.cs)
- [x] إضافة `const string LibyanPhonePattern` بقيمة `^(09\d{8}|\+2189\d{8})$`
- [x] إضافة `const string LibyanPhoneError` برسالة الخطأ العربية

### 1.3 — تحديث Tenant.cs
- [x] فتح [Tenant.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Tenant.cs)
- [x] استبدال Regex المكتوب يدوياً بـ `ValidationConstants.LibyanPhonePattern`
- [x] استبدال رسالة الخطأ بـ `ValidationConstants.LibyanPhoneError`

### 1.4 — تحديث BranchPhone.cs
- [x] فتح [BranchPhone.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Branchs/BranchPhone.cs)
- [x] استبدال Regex المكتوب يدوياً بـ `ValidationConstants.LibyanPhonePattern`
- [x] حذف `public Guid TenantId { get; set; }` (سيرث من BaseEntity)
- [x] حذف `public Tenant? Tenant { get; set; }` (سيرث من BaseEntity)

### 1.5 — تنظيف Branch.cs
- [x] فتح [Branch.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Branchs/Branch.cs)
- [x] حذف `public Guid TenantId { get; set; }`
- [x] حذف `public Tenant? Tenant { get; set; }`

### 1.6 — تنظيف Category.cs
- [x] فتح [Category.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Catalog/Category.cs)
- [x] حذف `public Guid TenantId { get; set; }`
- [x] حذف `public Tenant? Tenant { get; set; }`

### 1.7 — تنظيف Product.cs
- [x] فتح [Product.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Catalog/Product.cs)
- [x] حذف `public Guid TenantId { get; set; }`
- [x] حذف `public Tenant? Tenant { get; set; }`

### 1.8 — تنظيف ProductUnit.cs
- [x] فتح [ProductUnit.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Catalog/ProductUnit.cs)
- [x] حذف `public Guid TenantId { get; set; }`
- [x] حذف `public Tenant? Tenant { get; set; }`

### 1.9 — تنظيف ProductBarCode.cs
- [x] فتح [ProductBarCode.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Catalog/ProductBarCode.cs)
- [x] حذف `public Guid TenantId { get; set; }`
- [x] حذف `public Tenant? Tenant { get; set; }`

### 1.10 — تنظيف ProductImage.cs
- [x] فتح [ProductImage.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Catalog/ProductImage.cs)
- [x] حذف `public Guid TenantId { get; set; }`
- [x] حذف `public Tenant? Tenant { get; set; }`

### 1.11 — تنظيف Unit.cs
- [x] فتح [Unit.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Catalog/Unit.cs)
- [x] حذف `public Guid TenantId { get; set; }`
- [x] حذف `public Tenant? Tenant { get; set; }`

### 1.12 — إصلاح Enums (4 ملفات)
- [x] فتح [InvoiceStatus.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/InvoiceStatus.cs)
  - [x] تغيير `internal class` إلى `public enum`
  - [x] إضافة القيم: `Draft=0, Pending=1, Paid=2, PartiallyPaid=3, Cancelled=4, Voided=5`
- [x] فتح [PaymentStatus.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/PaymentStatus.cs)
  - [x] تغيير `internal class` إلى `public enum`
  - [x] إضافة القيم: `Pending=0, Completed=1, Failed=2, Refunded=3, PartiallyRefunded=4`
- [x] فتح [SalesOrderStatus.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/SalesOrderStatus.cs)
  - [x] تغيير `internal class` إلى `public enum`
  - [x] إضافة القيم: `Draft=0, Confirmed=1, Processing=2, Shipped=3, Delivered=4, Cancelled=5, Returned=6`
- [x] فتح [WarehouseType.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/WarehouseType.cs)
  - [x] تغيير `internal class` إلى `public enum`
  - [x] إضافة القيم: `Main=0, Branch=1, Transit=2, Consignment=3, Virtual=4`

### 1.13 — التحقق من المرحلة الأولى
- [x] تشغيل `dotnet build RetalSystemAPI.Models` — بنجاح بدون أخطاء (0 Error, 0 Warning)

---

## 🏗️ المرحلة الثانية — إعداد مشروع DataAccess

### 2.1 — إضافة مرجع Models
- [x] إضافة `<ProjectReference>` إلى Models في [DataAccess.csproj](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/RetalSystemAPI.DataAccess.csproj)

### 2.2 — إنشاء هيكل المجلدات
- [x] إنشاء مجلد `Context/`
- [x] إنشاء مجلد `Context/Configurations/`
- [x] إنشاء مجلد `Repositories/`
- [x] إنشاء مجلد `Repositories/Interfaces/`
- [x] إنشاء مجلد `Repositories/Implementations/`
- [x] إنشاء مجلد `Interceptors/`
- [x] إنشاء مجلد `Services/`
- [x] إنشاء مجلد `Extensions/`

---

## 🔌 المرحلة الثالثة — CurrentTenant & CurrentUser Services

### 3.1 — ICurrentTenantService
- [x] إنشاء [ICurrentTenantService.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Services/ICurrentTenantService.cs)

### 3.2 — CurrentTenantService
- [x] إنشاء [CurrentTenantService.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Services/CurrentTenantService.cs)

### 3.3 — ICurrentUserService
- [x] إنشاء [ICurrentUserService.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Services/ICurrentUserService.cs)

### 3.4 — CurrentUserService
- [x] إنشاء [CurrentUserService.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Services/CurrentUserService.cs)

---

## ⚙️ المرحلة الرابعة — Fluent API Configurations

### 4.1 — TenantConfiguration.cs
- [x] إنشاء [TenantConfiguration.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/Configurations/TenantConfiguration.cs)

### 4.2 — ApplicationUserConfiguration.cs
- [x] إنشاء [ApplicationUserConfiguration.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/Configurations/ApplicationUserConfiguration.cs)

### 4.3 — BranchConfiguration.cs
- [x] إنشاء [BranchConfiguration.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/Configurations/BranchConfiguration.cs)

### 4.4 — BranchPhoneConfiguration.cs
- [x] إنشاء [BranchPhoneConfiguration.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/Configurations/BranchPhoneConfiguration.cs)

### 4.5 — CategoryConfiguration.cs
- [x] إنشاء [CategoryConfiguration.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/Configurations/CategoryConfiguration.cs)

### 4.6 — ProductConfiguration.cs ⭐
- [x] إنشاء [ProductConfiguration.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/Configurations/ProductConfiguration.cs)

### 4.7 — ProductUnitConfiguration.cs
- [x] إنشاء [ProductUnitConfiguration.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/Configurations/ProductUnitConfiguration.cs)

### 4.8 — ProductBarCodeConfiguration.cs
- [x] إنشاء [ProductBarCodeConfiguration.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/Configurations/ProductBarCodeConfiguration.cs)

### 4.9 — ProductImageConfiguration.cs
- [x] إنشاء [ProductImageConfiguration.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/Configurations/ProductImageConfiguration.cs)

### 4.10 — UnitConfiguration.cs
- [x] إنشاء [UnitConfiguration.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/Configurations/UnitConfiguration.cs)

---

## 🗄️ المرحلة الخامسة — AppDbContext

### 5.1 — إنشاء AppDbContext.cs
- [x] إنشاء [AppDbContext.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/AppDbContext.cs)

### 5.2 — OnModelCreating
- [x] استدعاء `base.OnModelCreating(builder)`
- [x] استدعاء `builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly)`

### 5.3 — Global Query Filters
- [x] إضافة `HasQueryFilter` لـ Soft Delete لكل كيان يرث BaseEntity
- [x] إضافة `HasQueryFilter` للـ Multi-Tenancy لكل كيان يرث BaseEntity

### 5.4 — التحقق من AppDbContext
- [x] `dotnet build` ينجح بدون أخطاء

---

## 📦 المرحلة السادسة — Generic Repository Interface

### 6.1 — IRepository.cs
- [x] إنشاء [IRepository.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Repositories/Interfaces/IRepository.cs)

---

## 🔨 المرحلة السابعة — Generic Repository Implementation

### 7.1 — Repository.cs
- [x] إنشاء [Repository.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Repositories/Implementations/Repository.cs)

---

## 🔁 المرحلة الثامنة — Unit of Work

### 8.1 — IUnitOfWork.cs
- [x] إنشاء [IUnitOfWork.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Repositories/Interfaces/IUnitOfWork.cs)

### 8.2 — UnitOfWork.cs
- [x] إنشاء [UnitOfWork.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Repositories/Implementations/UnitOfWork.cs)

---

## 🎯 المرحلة التاسعة — AuditInterceptor

### 9.1 — AuditInterceptor.cs
- [x] إنشاء [AuditInterceptor.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Interceptors/AuditInterceptor.cs)

---

## 🔧 المرحلة العاشرة — DI Registration

### 10.1 — DataAccessServiceExtensions.cs
- [x] إنشاء [DataAccessServiceExtensions.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Extensions/DataAccessServiceExtensions.cs)

### 10.2 — تحديث Program.cs
- [x] تسجيل `AddDataAccess()` في [Program.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI/Program.cs)

---

## 🚀 المرحلة الحادية عشرة — Migration & Verification

### 11.1 — إنشاء Migration
- [x] إنشاء [DesignDbContextFactory.cs](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/DesignDbContextFactory.cs)
- [x] تشغيل `dotnet ef migrations add InitialCreate` بنجاح وتوليد ملفات الـ Migration

### 11.2 — التحقق والـ Build النهائي
- [x] `dotnet build` للـ Solution بالكامل — نجح بنجاح تـام (0 Errors)
