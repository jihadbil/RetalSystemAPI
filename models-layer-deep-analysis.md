# تقرير التحليل المعمق والشامل لطبقة النماذج (Models Layer Deep Analysis)

**المشروع:** RetalSystemAPI  
**التاريخ:** 2026-10-16  
**النطاق:** مشروع `RetalSystemAPI.Models` بالكامل (63 ملف C# موزعة على 12 مجلداً ونطاق عمل)، مع فحص الروابط المتقاطعة مع طبقة الوصول للبيانات `RetalSystemAPI.DataAccess` وطبقة نواقل البيانات `RetalSystemAPI.Models.DTOs` وطبقة الخدمات `RetalSystemAPI.Services`.  
**حالة التجميع (.NET 10):** Build succeeded — 0 أخطاء، 0 تحذيرات.  

---

## 1. الملخص التنفيذي (Executive Summary)

تمثل طبقة النماذج (`RetalSystemAPI.Models`) النواة المركزية ونطاق الأعمال الأساسي (Domain Core) لنظام رتال لإدارة الموارد ونقاط البيع. تغطي الطبقة دورة تشغيلية ومحاسبية متكاملة تشمل: إدارة المستأجرين والفروع، كتالوج الأصناف متعدد العبوات والباركودات، المشتريات والموردين، المبيعات ونقاط البيع، المستودعات وصالات العرض، وإدارة الخزائن والحسابات النقدية والمصروفات.

ورغم نجاح تجميع الطبقة بنسبة 100% وتحقيق تغطية توثيقية ممتازة بالتعليقات، إلا أن التحليل المعمق والشامل كشف عن **مجموعة من العيوب المعمارية، وفجوات أمنية في عزل المستأجرين، وتناقضات جذرية في أنواع البيانات والكميات، وفخاخ برمجية في التعدادات (Enums)، بالإضافة إلى أخطاء إملائية مؤثرة في أسماء الجداول والكيانات**.

### جدول التقييم المعماري للطبقة

| المحور الرقابي | التقييم | الخلاصة الفنية |
|---|:---:|---|
| **الهيكلة والوراثة وتعدد المستأجرين** | **6.5 / 10** | وراثة دائرية في `Tenant`، وفجوة أمنية في عزل `ApplicationUser` |
| **اتساق الأنواع والكميات (Data Types)** | **5.5 / 10** | تناقض حاد: كميات المبيعات والمخازن `int` بينما المشتريات `decimal` |
| **سلامة العلاقات وخصائص التنقل** | **7.5 / 10** | علاقات أحادية الاتجاه غير مكتملة، وتسميات شاذة لخصائص الملاحة |
| **تصميم التعدادات والقيم الافتراضية** | **6.0 / 10** | فخ القيمة الافتراضية الصفرية لـ 11 تعداداً يبدأ من 1 مع وجود Enum ميت |
| **التحقق من صحة البيانات (Validation)** | **6.5 / 10** | ازدواجية وتناقض في قواعد أرقام الهواتف وتشتت بين DataAnnotations و Fluent API |
| **المعايير النظيفة والاعتماديات** | **7.0 / 10** | تسرب حزم SQL Server و Tools إلى طبقة Domain، ونماذج فقيرة بالكامل |
| **التسمية والتدقيق اللغوي البرمجي** | **7.0 / 10** | خطآن إملائيان رئيسيان (`Branchs` و `Storge`) ممتدان للجداول والتعدادات |

---

## 2. الإحصائيات العامة لهيكل الطبقة

- **إجمالي ملفات C#:** 63 ملفاً برمجياً.
- **إجمالي الكيانات النطاقية (Domain Entities):** 37 كياناً.
- **إجمالي التعدادات (Enums):** 16 تعداداً تشغيلياً ومحاسبياً.
- **إجمالي حزم الصلاحيات والأذونات (Permissions):** 172 صريحاً موزعاً على 26 كلاس صلاحية.
- **توزيع الملفات حسب المجلدات:**
  - `Root`: 2 ملفات ([`Tenant.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Tenant.cs), [`ApplicationUser.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/ApplicationUser.cs)).
  - `Accounting`: 7 ملفات (الخزائن، الجلسات، المصروفات، طرفيات POS).
  - `Branchs`: 2 ملفات (الفروع وهواتفها).
  - `Catalog`: 6 ملفات (الأصناف، الفئات، الباركودات، الوحدات، الصور).
  - `Common`: 5 ملفات (الكيانات الأساسية، المعايرة، الثوابت، قواعد الكميات).
  - `Constants`: 1 ملف (الصلاحيات والأذونات).
  - `Customers`: 3 ملفات (الزبائن، الهواتف، دفتر الأستاذ).
  - `Enums`: 16 ملفاً.
  - `Purchase`: 7 ملفات (أوامر الشراء، فواتير الشراء وتفكيك العبوات، المرتجعات).
  - `Sales`: 4 ملفات (فواتير المبيعات، بنودها، المرتجعات).
  - `Suppliers`: 3 ملفات (الموردون، الهواتف، دفتر الأستاذ).
  - `Warehouses`: 7 ملفات (المستودعات، أرصدة الصالات، أرصدة التخزين، المناقلات، التسويات).

---

## 3. التحليل التفصيلي للعيوب والمشاكل المكتشفة

### 3.1 الخلل المعماري في الوراثة وتعدد المستأجرين (Inheritance & Multi-Tenancy)

#### [خلل 1] وراثة الكيان `Tenant` من `TenantBaseEntity` (Circular & Redundant Inheritance)
- **الملف المتأثر:** [`Tenant.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Tenant.cs#L15)
- **طبيعة المشكلة:**
  كلاس المستأجر مُعرّف كالتالي:
  ```csharp
  public class Tenant : TenantBaseEntity
  ```
  وبما أن [`TenantBaseEntity.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Common/TenantBaseEntity.cs#L8-L21) يرث من `BaseEntity` ويضيف:
  ```csharp
  public Guid TenantId { get; set; }
  public Tenant? Tenant { get; set; }
  ```
  فإن الكيان `Tenant` يرث حقلاً لمعرف المستأجر `TenantId`، وخاصية ملاحة ذاتية `Tenant? Tenant` تشير إلى نفسه!
- **الآثار الجانبية في النظام:**
  1. اضطر مطورو طبقة الوصول للبيانات إلى استبعاد خاصية الملاحة قسراً في [`TenantConfiguration.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/Configurations/TenantConfiguration.cs#L23):
     ```csharp
     builder.Ignore(t => t.Tenant);
     ```
  2. في [`AppDbContext.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/AppDbContext.cs#L191)، اضطر المطور لوضع استثناء يدوي خاص:
     ```csharp
     if (entityType.ClrType == typeof(Tenant)) { ... }
     else if (typeof(TenantBaseEntity).IsAssignableFrom(entityType.ClrType)) { ... }
     ```
     لأنه لولا هذا الاستثناء لتم تطبيق شرط `e.TenantId == CurrentTenantId` على المستأجر نفسه.
- **التوصية:**
  تعديل وراثة `Tenant` لترث مباشرة من `BaseEntity`:
  ```csharp
  public class Tenant : BaseEntity
  ```
  المستأجر هو الكيان الجذري (Root Tenant) ولا يتبع لأي مستأجر آخر، وبذلك تختفي خاصية `TenantId` الزائدة وخاصية الملاحة الدائرية تلقائياً.

---

#### [خلل 2 - حرج] الفجوة الأمنية لمرشحات الاستعلام في `ApplicationUser` (Multi-Tenancy Query Filter Gap)
- **الملف المتأثر:** [`ApplicationUser.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/ApplicationUser.cs#L14)
- **طبيعة المشكلة:**
  `ApplicationUser` يرث مباشرة من `IdentityUser` وليس من `BaseEntity` أو `TenantBaseEntity`.
  على الرغم من احتوائه على:
  ```csharp
  public Guid TenantId { get; set; }
  public Tenant? Tenant { get; set; }
  ```
  إلا أن محرك مرشحات الاستعلام العامة في [`AppDbContext.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/AppDbContext.cs#L207-L244) لا يفحص إلا الكيانات المشتقة من `TenantBaseEntity` أو `BaseEntity`.
- **الخطر الأمني والتشغيلي:**
  أي استعلام يتم عبر `_userManager.Users` أو `_context.Users` **لا يخضع إطلاقاً لمرشح المستأجر التلقائي** (`e.TenantId == CurrentTenantId`). وإذا نسي المطور في أي خدمة تصفية المستخدمين يدوياً بـ `.Where(u => u.TenantId == tenantId)`، فسيتمكن مستخدمو مستأجر معين من رؤية أو تعديل مستخدمي مستأجر آخر!
  بالإضافة إلى ذلك، يفتقر `ApplicationUser` لميزات الحذف الناعم (`IsDeleted`)، وطوابع التدقيق الموحدة (`CreatedAt`, `UpdatedAt`)، وقفل التزامن (`RowVersion`).
- **التوصية:**
  إما تطبيق `HasQueryFilter(u => u.TenantId == CurrentTenantId)` صراحة على `ApplicationUser` في `OnModelCreating`، أو جعل `ApplicationUser` يطبق واجهة موحدة `ITenantEntity` و `ISoftDeletable` وتعميم مرشحات الاستعلام بناءً على الواجهات (Interfaces).

---

#### [خلل 3] إلزامية الفرع في `ApplicationUser.BranchId` وتقييد الإدارة العليا
- **الملف المتأثر:** [`ApplicationUser.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/ApplicationUser.cs#L32)
- **طبيعة المشكلة:**
  الحقل مُعرّف كـ `public Guid BranchId { get; set; }` (غير قابل للقيمة الفارغة).
- **الأثر التشغيلي:**
  يفرض على كل مستخدم في النظام أن يكون مقيداً بفرع فيزيائي واحد فقط. هذا يمنع:
  1. وجود مستخدمين على مستوى الإدارة العامة للمنشأة (Tenant Admins / Auditors / General Managers) ممن يشرفون على كافة الفروع في آن واحد.
  2. إنشاء مستخدم رئيسي للمستأجر قبل إنشاء الفروع الأولى.
- **التوصية:**
  تعديل الخاصية لتصبح اختيارية:
  ```csharp
  public Guid? BranchId { get; set; }
  public Branch? Branch { get; set; }
  ```

---

### 3.2 التناقض الجذري في أنواع الكميات والدقة العددية (Quantity & Precision Type Divergence)

#### [خلل 4 - عالي الأهمية] صراع الأنواع بين قطاع المبيعات والمخازن (`int`) وقطاع المشتريات (`decimal`)
- **الملفات المتأثرة:**
  - المبيعات: [`SalesInvoiceItem.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Sales/SalesInvoiceItem.cs#L53)، [`SalesReturnItem.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Sales/SalesReturnItem.cs#L53) -> **`int Quantity`**
  - المخازن: [`ShowroomStock.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Warehouses/ShowroomStock.cs#L41)، [`StorgeStock.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Warehouses/StorgeStock.cs#L41)، [`StockTransferItem.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Warehouses/StockTransferItem.cs#L53)، [`StockAdjustmentItem.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Warehouses/StockAdjustmentItem.cs#L54-L66) -> **`int Quantity`**
  - المشتريات: [`PurchaseOrderItem.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Purchase/PurchaseOrderItem.cs#L41)، [`PurchaseInvoiceItem.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Purchase/PurchaseInvoiceItem.cs#L54)، [`PurchaseReturnItem.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Purchase/PurchaseReturnItem.cs#L53) -> **`decimal Quantity`**
- **التحليل الفني للمشكلة:**
  - في قطاع المشتريات: يسمح النظام بإدخال كميات كسرية (مثل 12.5 كجم أو 3.75 كرتونة).
  - ولكن بمجرد ترحيل الفاتورة وتغذية المخازن، يصطدم النظام بأن أرصدة المخازن (`StorgeStock.Quantity` و `ShowroomStock.Quantity`) هي أعمدة صحيحة `int`!
  - كلاس [`StockQuantity.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Common/StockQuantity.cs#L6-L16) يحاول معالجة ذلك برمي استثناء `ArgumentOutOfRangeException` إذا لم تكن الكمية عدداً صحيحاً (`quantity == decimal.Truncate(quantity)`).
  - **السؤال المعماري:** إذا كان النظام مصمماً لتخزين "أصغر وحدة كعدد صحيح" (Whole count of smallest unit)، فلماذا تم تعريف حقول المشتريات كـ `decimal`؟ وإذا كان النظام يدعم أوزان ومقاسات كسرية (لحوم، خضار، سوائل، أقمشة)، فلماذا قُيدت المبيعات والمخازن بـ `int`؟
- **التوصية:**
  توحيد نوع الكميات عبر كافة نماذج النظام (Purchase, Sales, Warehouses) ليكون **`decimal`** (بدقة `decimal(18,3)` أو `decimal(18,4)` في قاعدة البيانات)، مما يتيح للنظام دعم النشاط التجاري بالقطع الفردية والأوزان والمقاييس دون أي تعارض أو تحويلات قسرية قد تسبب فقداناً للبيانات أو استثناءات أثناء التشغيل.

---

### 3.3 فخاخ التعدادات والقيم الافتراضية الصفرية (Enum Zero-Value Default Traps & Dead Code)

#### [خلل 5] فخ القيمة الافتراضية الصفرية (`Zero-Value Default Trap`) في 11 تعداداً
- **طبيعة المشكلة في لغة C#:**
  القيمة الافتراضية لأي متغير Enum غير مسند هي `0` (`default(TEnum) == 0`).
  عندما لا يحتوي التعداد على القيمة `0`، فإن أي كائن جديد يُنشأ دون تمرير صريح لتلك الخاصية سيحمل القيمة `0` (قيمة رقمية صحيحة في الذاكرة ولكنها تمثل حالة غير مسماة وغير صالحة Unnamed/Invalid Enum Value).
- **التعدادات التي تبدأ من `1` وتفتقر للقيمة `0`:**
  1. [`CashSessionStatus`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/CashSessionStatus.cs#L13): `Open = 1`
  2. [`CustomerTransactionType`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/CustomerTransactionType.cs#L13): `SalesInvoice = 1`
  3. [`CustomerType`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/CustomerType.cs#L13): `Retail = 1`
  4. [`PurchaseOrderStatus`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/PurchaseOrderStatus.cs#L13): `Draft = 1`
  5. [`PurchaseReturnReason`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/PurchaseReturnReason.cs#L13): `Defective = 1`
  6. [`SafeType`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/SafeType.cs#L13): `Bank = 1`
  7. [`SalesReturnReason`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/SalesReturnReason.cs#L13): `Defective = 1`
  8. [`StockAdjustmentReason`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/StockAdjustmentReason.cs#L13): `InventoryCount = 1`
  9. [`StockTransferStatus`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/StockTransferStatus.cs#L13): `Draft = 1`
  10. [`SupplierTransactionType`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/SupplierTransactionType.cs#L13): `PurchaseInvoice = 1`
  11. [`TransactionDirection`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/TransactionDirection.cs#L13): `Inbound = 1`
  12. [`WarehouseType`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/WarehouseType.cs#L13): `Storge = 1`
- **النماذج المصابة بالخلل لعدم إسناد قيمة ابتدائية للخاصية:**
  - [`PurchaseOrder.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Purchase/PurchaseOrder.cs#L75): `public PurchaseOrderStatus Status { get; set; }` -> القيمة الافتراضية `0` (لا تمثل `Draft` بل حالة شاذة).
  - [`Warehouse.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Warehouses/Warehouse.cs#L38): `public WarehouseType Type { get; set; }` -> القيمة الافتراضية `0` (لا هي مخزن ولا صالة عرض).
  - [`SafeTransaction.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Accounting/SafeTransaction.cs#L35): `public TransactionDirection Direction { get; set; }` -> القيمة الافتراضية `0` (لا وارد ولا صادر).
  - [`SafeTransaction.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Accounting/SafeTransaction.cs#L59): `public FinancialDocumentType DocumentType { get; set; }` -> القيمة الافتراضية `0`.
  - [`CustomerTransaction.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Customers/CustomerTransaction.cs#L30): `public CustomerTransactionType TransactionType { get; set; }` -> القيمة الافتراضية `0`.
  - [`SupplierTransaction.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Suppliers/SupplierTransaction.cs#L30): `public SupplierTransactionType TransactionType { get; set; }` -> القيمة الافتراضية `0`.
  - [`StockAdjustment.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Warehouses/StockAdjustment.cs#L42): `public StockAdjustmentReason Reason { get; set; }` -> القيمة الافتراضية `0`.
- **التناقض الداخلي:** في حين أن [`InvoiceStatus.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/InvoiceStatus.cs#L13) و [`PaymentStatus.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/PaymentStatus.cs#L13) يبدآن بـ `0` (`Draft = 0`, `Pending = 0`)!
- **التوصية:**
  1. إسناد قيم ابتدائية صريحة في كافة الكيانات المصابة (مثال: `public PurchaseOrderStatus Status { get; set; } = PurchaseOrderStatus.Draft;`).
  2. توحيد فلسفة التعدادات: إما جعل أول حالة تساوي `0` أو إضافة قيمة `None = 0` أو `Unknown = 0` لمنع الحالات غير الصالحة عند التحويل من وإلى JSON أو في استعلامات قاعدة البيانات.

---

#### [خلل 6] تعداد غير مستخدم (Dead Enum): `PaymentStatus.cs`
- **الملف المتأثر:** [`PaymentStatus.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/PaymentStatus.cs)
- **طبيعة المشكلة:**
  التعداد معرف بكافة حالاته (Pending, Completed, Failed, Refunded, PartiallyRefunded). ولكنه غير مستخدم في أي كيان من كيانات النظام الـ 37 (لا فواتير مبيعات، ولا مشتريات، ولا حركات نقدية). كافة الفواتير تستخدم `InvoiceStatus`، وكافة الحركات تستخدم `PaymentMethod`.
- **التوصية:**
  إما استخدامه في سندات القبض والدفع وحركات الخزينة وربطه بـ DTOs، أو حذفه لتنظيف الطبقة من الأكواد المهملة.

---

### 3.4 عيوب العلاقات والمفاتيح الأجنبية وخصائص التنقل (Relational Integrity & Navigation Inconsistencies)

#### [خلل 7] انقطاع الملاحة ثنائية الاتجاه بين `CashSession` و `SalesInvoice`
- **طبيعة المشكلة:**
  - في [`SalesInvoice.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Sales/SalesInvoice.cs#L144-L150):
    ```csharp
    public Guid? CashSessionId { get; set; }
    public CashSession? CashSession { get; set; }
    ```
  - في المقابل، [`CashSession.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Accounting/CashSession.cs) لا يمتلك خاصية ملاحة تجميعية:
    ```csharp
    public ICollection<SalesInvoice> SalesInvoices { get; set; } = new List<SalesInvoice>();
    ```
- **الأثر التشغيلي:**
  عند جرد أو اعتماد جلسة الكاشير، لا يمكن للمطور الوصول إلى فواتير الجلسة مباشرة عبر كائن الجلسة (`session.SalesInvoices`)، بل يضطر لكتابة استعلام خارجي على جدول المبيعات.

---

#### [خلل 8] تسمية شاذة لخاصية التنقل في `ProductImage`
- **الملف المتأثر:** [`ProductImage.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Catalog/ProductImage.cs#L40-L46)
- **طبيعة المشكلة:**
  ```csharp
  public Guid? BarcodeId { get; set; }
  public ProductBarCode? BarcodeCode { get; set; }
  ```
  - المفتاح الأجنبي سمي `BarcodeId` بينما في سائر نماذج النظام يُسمى `ProductBarCodeId`.
  - خاصية الملاحة سميت `BarcodeCode` (اسم غريب ومربك يجمع بين الكلمتين دون داعٍ) بدلاً من `ProductBarCode` أو `BarCode`.
  - انعكس ذلك على Fluent API في [`ProductImageConfiguration.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.DataAccess/Context/Configurations/ProductImageConfiguration.cs#L40).
- **التوصية:**
  إعادة التسمية لتصبح:
  ```csharp
  public Guid? ProductBarCodeId { get; set; }
  public ProductBarCode? ProductBarCode { get; set; }
  ```

---

#### [خلل 9] تشتت حالة الأحرف (Casing Inconsistency) لـ `ProductBarCode`
- **طبيعة المشكلة:**
  - في `PurchaseOrderItem`, `PurchaseInvoiceItem`, `PurchaseInvoiceItemBreakdown`, `PurchaseReturnItem`, `SalesInvoiceItem`, `SalesReturnItem`, `StockTransferItem`, `StockAdjustmentItem`:
    المفتاح يسمى: **`ProductBarCodeId`** (حرف C كبير).
  - ولكن في [`StorgeStock.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Warehouses/StorgeStock.cs#L29-L35):
    المفتاح يسمى: **`ProductBarcodeId`** (حرف c صغير)! وخاصية الملاحة: **`ProductBarcode`**!
  - وفي `ProductImage.cs`: سمي **`BarcodeId`**!
- **التوصية:**
  توحيد التسمية بدقة: إما اعتماد `ProductBarCodeId` أو `ProductBarcodeId` عبر كامل الكود المصدري لمنع الأخطاء أثناء التعيين التلقائي في AutoMapper أو الاستعلامات الديناميكية.

---

#### [خلل 10] فجوة انقطاع ربط المستودع الرئيسي `StorgeStock` بالمنتج المباشر `ProductId`
- **طبيعة المشكلة:**
  - في [`ShowroomStock.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Warehouses/ShowroomStock.cs#L29): رصيد صالة العرض يرتبط مباشرة بالمنتج:
    ```csharp
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;
    ```
  - في [`StorgeStock.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Warehouses/StorgeStock.cs#L29): رصيد مستودع التخزين يرتبط بـ `ProductBarcodeId` فقط ولا يحتوي على `ProductId`!
  - في [`Product.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Catalog/Product.cs#L79): يحتوي المنتج على:
    ```csharp
    public ICollection<ShowroomStock> ShowroomStocks { get; set; } = new List<ShowroomStock>();
    ```
    ولكنه لا يحتوي على أي رابط مباشر بـ `StorgeStock`.
- **الأثر التشغيلي:**
  عند الاستعلام عن إجمالي رصيد منتج في المخازن، لا يمكن الانتقال مباشرة من الكيان `Product` إلى أرصدته في المستودعات الرئيسية إلا عبر المرور بجدول الباركودات `ProductBarCodes`، مما يعقد استعلامات التقارير ويزيد عدد عمليات الربط (Joins).

---

#### [خلل 11] تباين مسمى حقل اسم جهة الاتصال بين الهواتف
- **طبيعة المشكلة:**
  - في [`BranchPhone.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Branchs/BranchPhone.cs#L17): الحقل يسمى `public string? Name { get; set; }`
  - في [`SupplierPhone.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Suppliers/SupplierPhone.cs#L34): الحقل يسمى `public string? Name { get; set; }`
  - في [`CustomerPhone.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Customers/CustomerPhone.cs#L34): الحقل يسمى `public string? ContactName { get; set; }`
- **التوصية:**
  توحيد التسمية إلى `ContactName` أو `Name` في جميع جداول الهواتف الثلاثة لضمان الاتساق في DTOs والتطبيق.

---

### 3.5 التحقق من صحة البيانات وعدم اتساق أسلوب الـ Nullability

#### [خلل 12] تضارب صارخ في قواعد التحقق من أرقام الهواتف
- **الملفات المتأثرة:**
  - [`Tenant.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Tenant.cs#L39-L42):
    ```csharp
    [Required]
    [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public required string PhoneNumber { get; set; }
    ```
  - [`BranchPhone.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Branchs/BranchPhone.cs#L22-L25):
    ```csharp
    [Required]
    [RegularExpression(ValidationConstants.LibyanPhonePattern, ErrorMessage = ValidationConstants.LibyanPhoneError)]
    public required string PhoneNumber { get; set; }
    ```
  - [`CustomerPhone.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Customers/CustomerPhone.cs#L28):
    ```csharp
    public required string PhoneNumber { get; set; } // بدون أي DataAnnotation أو Regex!
    ```
  - [`SupplierPhone.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Suppliers/SupplierPhone.cs#L28):
    ```csharp
    public string PhoneNumber { get; set; } = null!; // بدون required وبدون DataAnnotation أو Regex!
    ```
- **الأثر التشغيلي:**
  المستأجر والفروع ملزمة بنمط الهاتف الليبي (`09XXXXXXXX` أو `+2189XXXXXXXX`)، بينما يُسمح في هواتف الزبائن والموردين بإدخال أي نصوص عشوائية، أو أرقام دولية، دون أي تحقق على مستوى النموذج!
- **التوصية:**
  تطبيق ثوابت التحقق [`ValidationConstants`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Common/ValidationConstants.cs) بشكل موحد عبر كافة الكيانات، أو السماح بصيغة دولية مرنة لكافة الأطراف.

---

#### [خلل 13] غياب مؤشر التفعيل/التعطيل `IsActive` في نموذج الصنف `Product`
- **الملف المتأثر:** [`Product.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Catalog/Product.cs)
- **طبيعة المشكلة:**
  جميع الكيانات المماثلة (`Tenant`, `Branch`, `Category`, `Customer`, `Supplier`, `Warehouse`, `Safe`, `ExpenseCategory`, `PosTerminal`) تحتوي على:
  ```csharp
  public bool IsActive { get; set; } = true;
  ```
  بينما الصنف `Product` يفتقر لهذه الخاصية تماماً.
- **الأثر التشغيلي:**
  إذا توقف إنتاج أو توريد صنف معين، أو رغبت المنشأة في إيقاف بيعه مؤقتاً دون حذفه (لأن حذفه عبر Soft Delete يخفيه كلياً)، فلن تتمكن من ذلك لأن النموذج لا يوفر مؤشراً لحالة النشاط.
- **التوصية:**
  إضافة `public bool IsActive { get; set; } = true;` إلى `Product.cs`.

---

#### [خلل 14] التباين بين أسلوب `required` وأسلوب `null!`
- **طبيعة المشكلة:**
  - في بعض النماذج الحديثة تم استخدام الكلمة المفتاحية `required` التي أضيفت في C# 11:
    - `public required string Name { get; set; }` في `Tenant`, `Product`, `Category`, `Customer`, `Safe`, `PosTerminal`.
  - وفي نماذج أخرى تم استخدام أسلوب التهيئة بـ `null!`:
    - `public string Name { get; set; } = null!;` في `Branch`, `Supplier`, `Warehouse`.
- **التوصية:**
  توحيد النمط البرمجي في المشروع بالكامل باستخدام `required` لكافة الخصائص النصية الإلزامية غير القابلة للقيمة الفارغة.

---

### 3.6 الأخطاء الإملائية ومخالفة معايير التسمية (Typos & Naming Standard Violations)

#### [خلل 15] خطأ إملائي في اسم مجلد الفروع: `Branchs`
- **المسار المتأثر:** `RetalSystemAPI.Models/Branchs/`
- **طبيعة المشكلة:**
  المجلد مُسمّى `Branchs` بزيادة s فقط، بينما الجمع الإملائي الإنجليزي الصحيح لقواعد اللغة هو **`Branches`**.
  وقد انعكس ذلك على فضاء الأسماء: `namespace RetalSystemAPI.Models.Branchs;`.

---

#### [خلل 16] خطأ إملائي فادح متكرر: `StorgeStock` و `WarehouseType.Storge`
- **الملفات المتأثرة:**
  - اسم الملف: [`StorgeStock.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Warehouses/StorgeStock.cs)
  - اسم الكلاس: `public class StorgeStock : TenantBaseEntity`
  - اسم الخاصية في `Warehouse`: `public ICollection<StorgeStock> WarehouseStocks`
  - اسم الـ DbSet في سياق البيانات: `DbSet<StorgeStock> StorgeStocks`
  - التعداد: [`WarehouseType.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/WarehouseType.cs#L13) -> `Storge = 1`
- **طبيعة المشكلة:**
  كلمة "تخزين" بالإنجليزية تكتب **`Storage`** بحرف `a` بعد الـ r. كتابتها `Storge` خطأ لغوي فادح (حيث أن كلمة Storge بالإنجليزية تشير إلى مفهوم يوناني فلسفي لمشاعر الألفة الأسرية!).
- **التوصية:**
  تصحيح الكلمة إلى **`StorageStock`** و **`WarehouseType.Storage`** عبر النماذج وسياق البيانات لرفع جودة وموثوقية الكود البرمجي للمشروع.

---

#### [خلل 17] اختصار غير قياسي: `WarehouseType.Show` بدلاً من `Showroom`
- **الملف المتأثر:** [`WarehouseType.cs`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/Enums/WarehouseType.cs#L19)
- **طبيعة المشكلة:**
  القيمة الثانية للتعداد سُميت `Show = 2`، بينما الكيان المقابل يسمى `ShowroomStock`.
- **التوصية:**
  تعديل القيمة إلى `Showroom = 2` ليتطابق التعداد مع مسمى الكيان ومصطلح صالة العرض.

---

### 3.7 انتهاكات البنية النظيفة والاعتماديات الحزمية (Clean Architecture & Package Smells)

#### [خلل 18] اعتماديات زائدة وغير ملائمة في `RetalSystemAPI.Models.csproj`
- **الملف المتأثر:** [`RetalSystemAPI.Models.csproj`](file:///c:/Users/Masoud/source/repos/RetalSystemAPI/RetalSystemAPI.Models/RetalSystemAPI.Models.csproj#L10-L20)
- **طبيعة المشكلة:**
  يحتوي ملف مشروع النماذج على الحزم التالية:
  ```xml
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.12" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.12" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="10.0.12" />
  ```
- **المخالفة المعمارية:**
  وفق مبادئ العمارة النظيفة (Clean Architecture) وفصل الاهتمامات:
  - طبقة النماذج (Domain / Models) يجب أن تكون نقية (Pure POCOs) ومستقلة تماماً عن مزود قاعدة بيانات محدد (Database Provider Agnostic).
  - حزم `SqlServer` و `Design` و `Tools` مكانها الطبيعي هو طبقة الوصول للبيانات `DataAccess` أو المشروع التنفيذي `RetalSystemAPI` (Startup Project).
  - وجود هذه الحزم في `Models` يفرض تحميل محرك SQL Server وأدوات الترحيل في كافة المشاريع التي تشير إلى النماذج (مثل DTOs أو Desktop)، مما يثقل حجم التجميعات وينتهك استقلالية النطاق.
- **التوصية:**
  حذف حزم `SqlServer` و `Design` و `Tools` من `RetalSystemAPI.Models.csproj`، والاكتفاء بحزم التجريد الأساسية أو DataAnnotations فقط.

---

#### [خلل 19] نماذج فقر الدم (Anemic Domain Models) وغياب قواعد حماية البيانات
- **طبيعة المشكلة:**
  كافة الكيانات الـ 37 مصممة كـ Anemic POCOs (خصائص `public { get; set; }` دون أي كبسلة أو دوال تحقق داخلية).
  على سبيل المثال:
  - يمكن لأي خدمة تعيين `PaidAmount` و `RemainingAmount` في `SalesInvoice` بقيم تتعارض مع `TotalAmount` دون أن يعترض الكيان.
  - يمكن تعديل `CurrentBalance` في `Safe` أو `Customer` دون تسجيل حركة مقابلة في `SafeTransaction` أو `CustomerTransaction`.
- **التوصية:**
  على المدى المتوسط، البدء في تحويل الكيانات الحساسة مالياً إلى Rich Domain Models أو إضافة دوال داخلية (Methods) لضبط الحالات والحسابات الذاتية، لحماية صحة البيانات ومنع الأخطاء البشرية في طبقات الخدمات.

---

## 4. المصفوفة الشاملة لتصنيف المشاكل والأولويات

| المعرف | المشكلة / الخلل | الموقع المتأثر | مستوى الخطورة | الأثر التشغيلي والتقني | أولوية الإصلاح |
|:---:|---|---|:---:|---|:---:|
| **SEC-01** | غياب مرشح الاستعلام التلقائي للمستأجرين في المستخدمين | `ApplicationUser.cs` | 🔴 **حرج جداً** | تسريب بيانات المستخدمين بين المستأجرين في حال نسيان الفلترة اليدوية | **P0 (فوري)** |
| **ARC-01** | وراثة دائرية للكيان `Tenant` من `TenantBaseEntity` | `Tenant.cs` | 🟠 **عالي** | وجود عمود `TenantId` وملاحة ذاتية دائرية تتطلب Ignore يدوي | **P1** |
| **DAT-01** | تناقض نوع الكميات: `int` في المبيعات والمخازن و `decimal` في المشتريات | كود المبيعات والمشتريات والمخازن | 🟠 **عالي** | تعارض وفشل في ترحيل الكميات الكسرية وتضارب حسابات المخزون | **P1** |
| **ENM-01** | فخ القيمة الافتراضية الصفرية لـ 11 تعداداً يبدأ من 1 دون إسناد أولي | التعدادات والكيانات المرتبطة | 🟠 **عالي** | حفظ قيمة 0 غير المعرفة في قاعدة البيانات وتشويه التقارير | **P1** |
| **DAT-02** | تضارب قواعد التحقق من أرقام الهواتف بين الفروع والزبائن والموردين | `CustomerPhone`, `SupplierPhone` | 🟡 **متوسط** | دخول بيانات عشوائية ومشوهة في هواتف العملاء والموردين | **P2** |
| **REL-01** | انقطاع الملاحة العكسية من جلسة الكاشير إلى فواتير المبيعات | `CashSession.cs` | 🟡 **متوسط** | إعاقة الوصول السلس لمبيعات الوردية أثناء الجرد والاعتماد | **P2** |
| **REL-02** | تسمية شاذة وغير متسقة لخاصية الملاحة `BarcodeCode` | `ProductImage.cs` | 🟡 **متوسط** | إرباك المطورين وكسر معايير EF Core في التسمية | **P2** |
| **MOD-01** | غياب خاصية `IsActive` في كيان الصنف `Product` | `Product.cs` | 🟡 **متوسط** | العجز عن تجميد أو إيقاف بيع صنف دون حذفه نهائياً | **P2** |
| **ARC-02** | إلزامية الفرع `BranchId` كـ `Guid` غير فارغ للمستخدم | `ApplicationUser.cs` | 🟡 **متوسط** | استحالة إنشاء مديري نظام أو مستأجر عامين دون فرع محدد | **P2** |
| **TYP-01** | خطأ إملائي فادح متكرر `StorgeStock` و `WarehouseType.Storge` | مجلد وموديل المخازن والتعدادات | 🔵 **تحسيني / جودة** | خطأ لغوي ممتد لجداول الداتابيز وواجهات الـ API | **P3** |
| **TYP-02** | خطأ إملائي في اسم المجلد `Branchs` بدلاً من `Branches` | مجلد `Branchs/` | 🔵 **تحسيني / جودة** | مخالفة القواعد الإملائية القياسية في أسماء الحزم | **P3** |
| **PKG-01** | تسرب حزم SQL Server و Design و Tools إلى مشروع النماذج | `RetalSystemAPI.Models.csproj` | 🔵 **معماري** | انتهاك استقلالية النطاق وتضخيم اعتماديات المشاريع الأخرى | **P3** |
| **ENM-02** | وجود كود تعداد ميت غير مستخدم `PaymentStatus` | `PaymentStatus.cs` | ⚪ **منخفض** | بقاء أكواد غير مستغلة في بنية المشروع | **P3** |

---

## 5. الخاتمة وخطة العمل المقترحة

طبقة النماذج في مشروع `RetalSystemAPI` تتمتع ببنية غنية ومصممة بعناية وتغطي كافة متطلبات النظام المتطورة. ومع ذلك، فإن معالجة المشكلات المصنفة أعلاه (خاصة **SEC-01** المتعلقة بأمان المستخدمين، و **ARC-01** المتعلقة بوراثة المستأجر، و **DAT-01** المتعلقة بتوحيد نوع الكميات إلى `decimal`) ستنقل المشروع إلى مستوى المؤسسات الكبرى (Enterprise Grade) وتضمن ثبات المعاملات وتناسق العمليات المالية والمخزنية دون أي مخاطر مستقبلية.
