# 📋 قائمة مهام تنفيذ طبقة الـ MAUI — RetalSystem (Emerald Management Pro)

تضم هذه القائمة جميع المهام التفصيلية المحددة للبدء والتنفيذ خطوة بخطوة لبناء تطبيق **.NET MAUI** المتوافق مع **الوضع الفاتح (Light Mode)** بالكامل ودعم العربية **RTL**.

---

## 🏗️ المرحلة 1: تهيئة المشروع وإضافة المراجع والحزم

- [x] **1.1 إعداد مراجع المشروع (Project References & NuGets)**
  - [x] إضافة مرجع مشروع DTOs إلى `RetalSystemAPI.MAUI.csproj`:
    `<ProjectReference Include="..\RetalSystemAPI.Models.DTOs\RetalSystemAPI.Models.DTOs.csproj" />`
  - [x] إضافة حزمة `CommunityToolkit.Mvvm` (الإصدار 8.4.0 أو الأحدث).
  - [x] إضافة حزمة `Microsoft.Extensions.Http` لإدارة الـ `HttpClient`.

- [x] **1.2 إضافة وتكوين الخط العربي (Cairo Font)**
  - [x] تسجيل الخطوط في `MauiProgram.cs` باستخدام `.ConfigureFonts(fonts => { fonts.AddFont("OpenSans-Regular.ttf", "Cairo"); fonts.AddFont("OpenSans-Semibold.ttf", "CairoBold"); })`.

- [x] **1.3 تهيئة حاقن الخدمات (Dependency Injection in `MauiProgram.cs`)**
  - [x] تسجيل `AuthHeaderHandler` كـ Transient.
  - [x] تسجيل خدمات `HttpClient` وتعيين الـ BaseAddress والـ Handler.
  - [x] تسجيل خدمات الـ API و الـ ViewModels و الـ Views في `builder.Services`.

---

## 🎨 المرحلة 2: تطبيق نظام التصميم Emerald Management Pro (Light Mode Only & RTL)

- [x] **2.1 إعداد رموز التصميم في `App.xaml`**
  - [x] اللون الأساسي (Primary): `#38e07b`
  - [x] خلفية التطبيق (Background): `#f6f8f7`
  - [x] خلفية البطاقات والأسطح (Surface): `#ffffff`
  - [x] النص الأساسي (Text Primary): `#111827`
  - [x] النص الثانوي (Text Muted): `#6b7280`
  - [x] الحدود (Borders): `#e5e7eb`

- [x] **2.2 تثبيت الوضع الفاتح (Disable Dark Mode)**
  - [x] ضبط `UserAppTheme = AppTheme.Light` في `App.xaml.cs` لمنع التبديل إلى الوضع المظلم نهائياً.

- [x] **2.3 إنشاء الأنماط الموحدة في `Resources/Styles/Styles.xaml`**
  - [x] نمط الأزرار الأساسية (Primary Emerald Button) بحواف `8px` ونصوص بيضاء.
  - [x] نمط الأزرار الفرعية (Outlined Button) بحواف `8px` وحاشية `#e5e7eb`.
  - [x] نمط بطاقات العرض (Card Border/Frame) بحواف `12px` وخلفية `#ffffff` وظل خفيف.
  - [x] نمط حقول النصوص (Entry & Editor) بحواف `8px` وخلفية ناصعة ومحاذاة RTL.
  - [x] نمط الشارات (Status Badges) بحواف دائرية كاملة `9999px`.

- [x] **2.4 ضبط الهيكل العام والاتجاه في `AppShell.xaml`**
  - [x] إضافة الخاصية `FlowDirection="RightToLeft"` لضمان القوائم الشجرية والجانبية بأسلوب عربي متكامل.

---

## 🔐 المرحلة 3: الخدمات والبنية التحتية للاتصال بالشبكة (Core & API Services)

- [x] **3.1 إنشاء معالج التوثيق `AuthHeaderHandler.cs`**
  - [x] استخراج Token المخزن وإرفاقه في ترويسة الطلب `Authorization: Bearer <token>`.

- [x] **3.2 إنشاء خدمة حالة التوثيق `AuthStateService.cs`**
  - [x] حفظ واسترجاع ومسح `jwt_token` و `tenant_id` عبر `SecureStorage`.

- [x] **3.3 إنشاء خدمة المصادقة `AuthApiService.cs`**
  - [x] `LoginAsync(LoginDto dto)` -> `POST api/auth/login`
  - [x] `RegisterAsync(RegisterDto dto)` -> `POST api/auth/register`
  - [x] `LogoutAsync()` -> `POST api/auth/logout`

- [x] **3.4 إنشاء خدمة المستأجرين `TenantApiService.cs`**
  - [x] `GetMyProfileAsync()`, `UpdateMyProfileAsync(UpdateTenantDto dto)`
  - [x] `GetAllAsync()`, `GetPagedAsync(int page, int size)`
  - [x] `GetByIdAsync(Guid id)`, `CreateAsync(CreateTenantDto dto)`, `UpdateAsync(Guid id, UpdateTenantDto dto)`, `DeleteAsync(Guid id)`
  - [x] `ToggleActiveStatusAsync(Guid id)`

- [x] **3.5 إنشاء خدمة الفروع `BranchApiService.cs`**
  - [x] `GetAllAsync()`, `GetPagedAsync(int page, int size)`, `GetByIdAsync(Guid id)`
  - [x] `CreateAsync(CreateBranchDto dto)`, `UpdateAsync(Guid id, UpdateBranchDto dto)`, `DeleteAsync(Guid id)`
  - [x] `ToggleActiveStatusAsync(Guid id)`

- [x] **3.6 إنشاء خدمة التصنيفات `CategoryApiService.cs`**
  - [x] `GetAllAsync()`, `GetRootsAsync()`, `GetChildrenAsync(Guid parentId)`, `GetPagedAsync(int page, int size)`
  - [x] `GetByIdAsync(Guid id)`, `CreateAsync(CreateCategoryDto dto)`, `UpdateAsync(Guid id, UpdateCategoryDto dto)`, `DeleteAsync(Guid id)`
  - [x] `ToggleActiveStatusAsync(Guid id)`

- [x] **3.7 إنشاء خدمة وحدات القياس `UnitApiService.cs`**
  - [x] `GetAllAsync()`, `GetByIdAsync(Guid id)`, `CreateAsync(CreateUnitDto dto)`, `UpdateAsync(Guid id, UpdateUnitDto dto)`, `DeleteAsync(Guid id)`

- [x] **3.8 إنشاء خدمة المنتجات الكبرى `ProductApiService.cs`**
  - [x] `GetPagedAsync(int page, int size, Guid? categoryId)`
  - [x] `SearchAsync(string q)`, `GetByBarCodeAsync(string barCode)`, `GetByIdAsync(Guid id)`
  - [x] `CreateAsync(CreateProductDto dto)`, `UpdateAsync(Guid id, UpdateProductDto dto)`, `DeleteAsync(Guid id)`
  - [x] **إدارة Excel:** `ExportExcelAsync()`, `DownloadTemplateAsync()`, `ImportExcelAsync(Stream fileStream, string fileName)`
  - [x] **إدارة الوحدات:** `GetProductUnitsAsync(Guid productId)`, `AddProductUnitAsync(Guid productId, CreateProductUnitDto dto)`, `RemoveProductUnitAsync(Guid productUnitId)`, `SetDefaultProductUnitAsync(Guid productUnitId)`
  - [x] **إدارة الباركودات:** `GetProductBarCodesAsync(Guid productId)`, `AddProductBarCodeAsync(Guid productId, CreateProductBarCodeDto dto)`, `RemoveProductBarCodeAsync(Guid barCodeId)`
  - [x] **إدارة الصور:** `GetProductImagesAsync(Guid productId)`, `UploadProductImageAsync(Guid productId, Stream imageStream, string fileName, bool isDefault, Guid? barcodeId)`, `RemoveProductImageAsync(Guid imageId)`, `SetDefaultProductImageAsync(Guid imageId)`

---

## 🧠 المرحلة 4: طبقة نماذج العرض (ViewModels)

- [x] **4.1 ViewModels الخاصة بالحساب والمصادقة**
  - [x] `LoginViewModel.cs`: إدارة تسجيل الدخول وتخزين التوكن.
  - [x] `RegisterViewModel.cs`: إنشاء حساب جديد.

- [x] **4.2 `DashboardViewModel.cs`**
  - [x] إحصائيات سريعة، عدد الأصناف، الفروع النشطة، الترحيب بالمستخدم.

- [x] **4.3 `TenantsViewModel.cs`**
  - [x] عرض قائمة المستأجرين والتحكم بالحالة والتفعيل.

- [x] **4.4 `BranchesViewModel.cs`**
  - [x] عرض الفروع وإضافة وتعديل فرع.

- [x] **4.5 `CategoriesViewModel.cs`**
  - [x] عرض الهيكل الهرمي، التصنيفات الرئيسية والفرعية والإضافة/التعديل.

- [x] **4.6 `UnitsViewModel.cs`**
  - [x] إدارة قائمة وحدات القياس.

- [x] **4.7 `ProductsViewModel.cs`**
  - [x] قائمة الأصناف الصفحية، شريط البحث بالاسم والباركود، تصفية التصنيفات.
  - [x] أوامر تصدير واستيراد ملفات Excel وتنزيل النموذج.

- [x] **4.8 `ProductDetailViewModel.cs`**
  - [x] عرض تفاصيل المنتج، علامات تبويب للوحدات والباركودات والصور.

---

## 📱 المرحلة 5: طبقة الواجهات (Views & XAML Pages)

- [x] **5.1 واجهات الحسابات**
  - [x] `Views/Auth/LoginPage.xaml`: تصميم ناعم بخلفية `#f6f8f7` وزر emerald `#38e07b`.
  - [x] `Views/Auth/RegisterPage.xaml`: نموذج التسجيل بالعربية RTL.

- [x] **5.2 `AppShell.xaml`**
  - [x] إعداد القائمة الجانبية RTL وعناصر التنقل للصفحات.

- [x] **5.3 `Views/DashboardPage.xaml`**
  - [x] كروت الإحصائيات مع انحناءات `12px` وظل ناعم.

- [x] **5.4 `Views/Tenants/TenantsPage.xaml`**
  - [x] بطاقات المستأجرين وأزرار الإجراءات السريعة.

- [x] **5.5 `Views/Branches/BranchesPage.xaml`**
  - [x] قائمة الفروع وعرض هواتف التواصل في بطاقات.

- [x] **5.6 `Views/Catalog/CategoriesPage.xaml`**
  - [x] شبكة/قائمة التصنيفات بطريقة بصرية مميزة.

- [x] **5.7 `Views/Catalog/UnitsPage.xaml`**
  - [x] واجهة سريعة لإدارة وحدات القياس العامة.

- [x] **5.8 `Views/Catalog/ProductsPage.xaml`**
  - [x] واجهة الأصناف الشاملة (البحث، الفلترة، الجدول/البطاقات، أزرار Excel، والصفحات).

- [x] **5.9 `Views/Catalog/ProductDetailPage.xaml`**
  - [x] واجهة تفاصيل المنتج مع معرض الصور والباركودات ومعاملات التحويل للوحدات.

---

## 🔍 المرحلة 6: التحقق والتجميع والتشغيل

- [x] **6.1 التجميع والاختبار المباشر (Build Verification)**
  - [x] تشغيل `dotnet build RetalSystemAPI.MAUI/RetalSystemAPI.MAUI.csproj -f net10.0-windows10.0.19041.0` والتأكد من عدم وجود أخطاء.
- [x] **6.2 مراجعة الهوية والتصاميم**
  - [x] التحقق من تطبيق الهوية الزمردية `#38e07b` بالكامل في الوضع الفاتح والاتجاه RTL.
