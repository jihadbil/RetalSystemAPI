# خطة إصلاح مشاكل N+1 وتحميل الجداول الكاملة في الذاكرة

## الهدف والقرارات المعتمدة
إصلاح كل مشاكل N+1 وتحميل الجداول الكاملة المكتشفة في التحليل السابق، **دون تغيير أي سلوك وظيفي** (نفس النتائج، نفس الرسائل، نفس قواعد العمل). القرارات التصميمية (باتخاذ أفضل اجتهاد لعدم ورود إجابات):
- **النطاق الكامل**: N+1 + تقليم Includes + الداشبورد + الإكسل + خدمات المخازن.
- **الداشبورد**: تجميع SQL-side عبر استعلامات مجمّعة (SumAsync/GroupBy) بدل تحميل الكيانات.
- **الإكسل**: إصلاح جراحي دون تجزئة الملف (ال God Class خارج نطاق هذه الخطة).

قاعدة حاكمة: كل المفاتيح المجمّعة تُجلب بـ **استعلام واحد يحتوي `Contains` على قائمة المعرفات** ثم تُبنى `Dictionary` — الحلقات بعدها تعمل على الذاكرة فقط. البنية (SQL Server + EF Core 10 + SplitQuery افتراضي) تدعم ذلك.

---

## المرحلة 0 — بنية تحتية جديدة في DataAccess (تمكين الباقي)

**الملفات:** `Repositories/Interfaces/IRepository.cs`، `Repositories/Implementations/Repository.cs`

إضافة ثلاث دوال عامة (تنفيذها ميكانيكي، بنفس أسلوب الموجود):

```csharp
// 1. نسخة متتبعة من FindAsync — لجلب أرصدة المخزون دفعة واحدة ثم تعديلها (يحل N+1 مع الحفاظ على RowVersion/التتبع)
Task<IReadOnlyList<T>> FindTrackedAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

// 2. تحميل أعمدة نحيفة فقط (يحل تحميل الجداول الكاملة: نحتاج Id/Name/BarCode لا الكيانات كاملة)
Task<IReadOnlyList<TResult>> SelectAsync<TResult>(Expression<Func<T, TResult>> selector, CancellationToken ct = default) where TResult : class;
```

ملاحظات:
- `FindTrackedAsync` = نفس جسم `FindAsync` الحالي (سطر 64-67) دون `AsNoTracking()`.
- `SelectAsync` = `_dbSet.AsNoTracking().Select(selector).ToListAsync(ct)` — يخدم التصدير، التحقق من الإكسل، وبذر المخزون.
- `AddRangeAsync` موجودة أصلاً (IRepository.cs:161) — ستُستخدم بدل حلقات AddAsync.
- لا حاجة لأي تغيير في UnitOfWork أو Specifications.

## المرحلة 1 — إصلاح N+1 في خدمات حركة المخزون (7 خدمات، القلب)

النمط الموحد المستبدَل في كل خدمة (مثال من SalesInvoiceService.CreateAsync):
- **قبل:** لكل بند → `FirstOrDefaultAsync` للباركود الافتراضي + `FirstOrDefaultAsync` للرصيد (فحص) ثم `FirstOrDefaultAsync` مجدداً للخصم = 2-3 استعلام × عدد البنود.
- **بعد:** جمع كل `ProductId` و`ProductBarCodeId` من البنود مرة واحدة → استعلامان (`ProductBarCodes.FindAsync(productIds.Contains)` و `ShowroomStocks.FindTrackedAsync(wh && productIds.Contains)` أو `StorgeStocks.FindTrackedAsync(wh && barcodeIds.Contains)`) → `Dictionary` بالمفتاح المركب → كل الفحص والخصم من الذاكرة، و`Update()` على كيانات متتبعة (يحل أيضاً مشكلة عدم التتبع في UpdateStatusAsync/DeleteAsync كأثر جانبي ضمن نفس الأسطر — بلا تغيير سلوكي).

**الملفات والعمليات:**

| الملف | العملية | الإصلاح |
|---|---|---|
| `Sales/SalesInvoiceService.cs` | CreateAsync (139-231)، UpdateAsync (265-405)، UpdateStatusAsync (441-470)، DeleteAsync (489-519) | دفعة باركود افتراضي + دفعة أرصدة Show/Storge لكل دورة (فحص وخصم واسترجاع) |
| `Sales/SalesReturnService.cs` | CreateAsync (146-221)، DeleteAsync (241-271) | دفعة باركود + دفعة أرصدة (إضافة/خصم) |
| `Purchase/PurchaseInvoiceService.cs` | `AdjustInvoiceItemsStockAsync` (312-421) | دفعة أرصدة واحدة لكل (مستودع، مجموعة باركودات Breakdowns+Items) — يزيل أيضاً النمط المزدوج `FirstOrDefault ثم GetById` (327-337/366-377)؛ `UpdateProductsCostAsync` (426-445): دفعة `Products.FindTrackedAsync(ids.Contains)` |
| `Purchase/PurchaseReturnService.cs` | CreateAsync (158-235): دفعة منتجات (162) + دفعة باركودات (171) + دفعة أرصدة فحص (185) + دفعة خصم (223)؛ DeleteAsync (256-276) كذلك |
| `Purchase/PurchaseOrderService.cs` | UpdateStatusAsync/استلام (249-375): دفعة أرصدة + دفعة باركود→منتج (295، 351) + دفعة منتجات للفاتورة المولدة |
| `Warehouses/StockTransferService.cs` | UpdateStatusAsync (202-302): فحص المصدر بدفعة واحدة، ثم الخصم من المصدر والإضافة للوجهة بدفعتين (مصدر/وجهة) بدل استعلامين لكل بند |
| `Warehouses/StockAdjustmentService.cs` | CreateAsync (130-176): دفعة أرصدة Show/Storge |

**حفظ الدلالات:** اختيار "الباركود الافتراضي" الحالي `FirstOrDefault` بلا ترتيب (غير حتمي أصلاً) — سيُبنى بـ `ToLookup(ProductId)` وأخذ الأول (نفس درجة الحرية، توثيق بتعليق قصير). ترتيب رسائل الأخطاء يبقى كما هو (نفس ترتيب فحص البنود).

## المرحلة 2 — تقليم Includes لقوائم الـ Summary (Overfetching)

تقرير وكيل فحص الـ Mappings أكد: **ولا** SummaryDto يقرأ `Items.Product` أو `Items.ProductBarCode` — كلها تحتاج فقط التنقلات المباشرة (Branch/Warehouse/Customer/Supplier/From/ToWarehouse/OriginalInvoice/PurchaseInvoice) + `Items.Count`. الـ Includes العميقة لخدمة التفاصيل فقط.

إضافة مواصفات "قوائم" نحيفة بجانب الموجودة (المفصلة تبقى لـ GetById/GetByNumber):

| ملف المواصفات | المواصفة الجديدة | تحذف |
|---|---|---|
| `Sales/Specifications/SalesInvoiceSpecifications.cs` | `SalesInvoiceListSpec` (4 منشئات مطابقة) | `Items.Product`، `Items.ProductBarCode` |
| `Sales/Specifications/SalesReturnSpecifications.cs` | `SalesReturnListSpec` | `Items.Product`، `Items.ProductBarCode` (يبقى OriginalInvoice) |
| `Warehouses/Specifications/StockTransferSpecifications.cs` | `StockTransferListSpec` | `Items.Product`، `Items.ProductBarCode` |
| `Purchase/Specifications/PurchaseOrderSpecifications.cs` | منشئ فلترة قائمة | `Items.ProductBarCode.Product` |
| `Purchase/Specifications/PurchaseReturnSpecifications.cs` | `PurchaseReturnListSpec` | `Items.*` |
| `Catalog/Specifications/CategorySpecifications.cs` | `CategoryListSpec` | **`Products`** (كارثية لقائمة تصنيفات) — تبقى ParentCategory/SubCategories |
| `Catalog/Implementations/ProductBarCodeService.cs` | تعديل `AllProductBarCodesSpec` | `ProductImages` (يبقى Product) |

الخدمات توجّه `GetAllAsync/GetPagedAsync` للمواصفة النحيفة؛ `PurchaseInvoiceFilterSpec` سليم أصلاً (Includes: Items فقط).
**الملفات المتأثرة إضافياً:** SalesInvoiceService، SalesReturnService، StockTransferService، PurchaseOrderService، PurchaseReturnService، CategoryService، ProductBarCodeService (استدعاءات فقط).

## المرحلة 3 — DashboardService: تجميع SQL-side

**الملف:** `Dashboard/Implementations/DashboardService.cs`

1. **`GetSummaryAsync` — ربح اليوم (56-64):** استبدال تحميل فواتير اليوم ببنودها بـ **استعلامي تجميع**: `SumAsync(TotalAmount)` + عدد + `SalesInvoiceItems` المرتبطة بفواتير اليوم `SumAsync(LineTotal - Quantity*UnitCost)` (ترجمة SQL صحيحة لطرح أعمدة). AverageOrderValue من النتيجتين.
2. **`GetSalesTrendInternalAsync` (204-247):** استبدال تحميل كل فواتير N أيام ببنودها بـ **GroupBy على التاريخ داخل SQL**: تجميعان (`Sum(TotalAmount)+Count` على الفواتير، و`Sum(LineTotal - Qty*UnitCost)` على البنود عبر Join) بمفتاح `InvoiceDate.Date` → `Dictionary<DateTime,(sales,profit,orders)>` → ملء مصفوفة الأيام السبعة بالأسماء العربية في الذاكرة (نفس حدود الأيام المحلية تُمرر كمعاملات — **نفس السلوك الزمني الحالي**).
3. **`DownloadTemplate`** (لا علاقة) — يبقى `TopSellingProducts` كما هو (مجمّع في SQL أصلاً)، و`LowStockAlerts` جيد (Take 10).
4. `DateTime.Now/UtcNow` **خارج النطاق** (سلوك وظيفي) — لا يُلمس.

## المرحلة 4 — ProductExcelService: الإصلاح الجراحي

**الملف:** `Catalog/Implementations/ProductExcelService.cs` (1645 سطراً)

1. **[الأخطر] سطر 911:** `GetAllTrackedAsync(ProductUnits)` داخل حلقة الصفوف → **رفعه خارج الحلقة** (تحميل واحد قبل الحلقة + إضافة الجديد إليه في الذاكرة).
2. **مسوحات خطية لكل صف → Dictionary/Lookup:** `existingShowroomStocks` (986)، `existingStorageStocks` (1011)، وفي Multi-Sheet: `existingCategories` (1085)، `existingProducts` (1168-1174)، `existingBarcodes` (1236-1361)، `productsWithBarcodes` (1337)، وفي ValidateExcel: 403 و520. كل قائمة تُبنى مرة واحدة كمفتاح مركب (مثل `WarehouseId_ProductId`) أو `ILookup` — البحث O(1).
3. **التحميلات الكاملة → أعمدة نحيفة عبر `SelectAsync` الجديدة:**
   - `ExportProductsToExcelAsync` (42-48): 7 جداول كاملة → أعمدة مستخدمة فقط: Products(Id,Name,CostPrice,SalePrice,CategoryId,Description)، BarCodes(Id,ProductId,BarCode)، Categories(Id,Name)، Units(Id,Name)، ProductUnits(ProductId,UnitId,ConversionFactor,IsDefault)، الأرصدة كمجموعات مفاتيح فقط.
   - `ValidateExcelAsync` (264-266): → Products(Id,Name)، BarCodes(BarCode,ProductId)، Categories(Name).
   - `DownloadTemplateAsync` (155): `Categories.GetAllAsync` لقراءة أول اسم → استعلام صف واحد عبر `FirstOrDefaultAsync` الموجود.
4. **التحميلات المتتبعة في الاستيراد تبقى** (كيانات تُعدَّل فعلاً: Products/BarCodes/Stocks/Categories/Units) — لكن تُبنى فهارسها القاموسية قبل الحلقات (بند 2). `SaveTrackedChangesAsync` ومعالجة RowVersion (1586-1628) **لا تُلمس**. `AddRangeAsync` بدل حلقات الإدراج المسرحية في نقاط البذر (النصوص 749/760) إن كان بديلاً حرفياً.

## المرحلة 5 — WarehouseService وStockService وUserService

1. **`WarehouseService.CreateAsync` (93، 110):** `ProductBarCodes.GetAllAsync`/`Products.GetAllAsync` كاملة → `SelectAsync(bc => bc.Id)` / `SelectAsync(p => new { p.Id, p.TenantId })` + `AddRangeAsync` واحدة لكل الدفعات (سلوك البذر نفسه).
2. **`StockService`** (GetStorgeStocksByWarehouseAsync 52-87 و Showroom 186-221): **السلوك يبقى كما هو** (بذر صفوف صفرية لتظهر في الشبكة — قرار وظيفي قائم) لكن: مفاتيح الأرصدة الحالية عبر `SelectAsync` بدل كيانات كاملة، مفاتيح الباركودات/المنتجات كاملة بـ `SelectAsync`، والمفقود يُدرج بـ `AddRangeAsync` واحدة + SaveChanges واحدة.
3. **`UserService`:**
   - `GetPagedUsersAsync` (83-86): `GetRolesAsync` لكل مستخدم → **استعلامان**: `UserRoles` (Join مع `Roles`) لمعرفات صفحة المستخدمين + `Users` — عبر `_context.UserRoles` (AppDbContext يرث `IdentityDbContext` — مؤكد من الاستكشاف).
   - `GetRolesAsync` (359-373): `GetClaimsAsync` لكل دور → استعلام `RoleClaims` واحد مفلتر بـ `Permissions.ClaimType`.
   - إنشاء الأدوار الافتراضية عند القراءة (350-357): **خارج النطاق** (سلوك وظيفي — يُوثَّق كملاحظة).

## ما يبقى خارج النطاق صراحةً
- تجزئة ProductExcelService، مشاكل المعاملات/التزامن P0 الأخرى، الآن→UTC، تسجيل ILogger، إنشاء مشروع اختبارات — (موثقة في تقرير التحليل السابق، بند منفصل).
- `CategoryService.IsCircularReferenceAsync` (استعلام لكل عمق شجرة — محدود بعمق الشجرة، غير حرج) يُترك كما هو.

## التحقق
1. `dotnet build` للمشروع بأكمله بعد كل مرحلة (0 أخطاء/0 تحذيرات — الحالة الحالية نظيفة).
2. مراجعة ذاتية مطابقة سلوكية لكل خدمة عدّلتها (نفس الرسائل وأكواد الأخطاء وترتيبها).
3. لا يوجد مشروع اختبارات في الحل — سأختم بملخص مفصّل بالتغييرات + جدول "قبل/بعد" بعدد الاستعلامات لكل عملية لكل خدمة، كي تتمكن من مراجعة أي عملية يدوياً.

## ترتيب التنفيذ والملفات (16 ملفاً)
0 → 1 → 2 → 3 → 4 → 5 (المرحلة 0 أولاً لأن الجميع يعتمد عليها)، وبناء تحققي بعد كل مرحلة.