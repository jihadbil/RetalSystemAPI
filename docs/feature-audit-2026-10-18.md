# مراجعة النظام حسب الميزات — 18 أكتوبر 2026

> هذا التقرير يصف الحالة وقت المراجعة. الإصلاحات اللاحقة ونتائج التحقق موثقة في [سجل الإصلاحات](feature-fixes-2026-10-18.md).

## النتيجة والنطاق

تم تقسيم النظام إلى **16 ميزة** وتتبع مساراتها من `Models` وتهيئة EF Core إلى `DTOs` والمستودعات والخدمات وواجهات API ثم `retalsystem.web`.

النتيجة: **24 مشكلة موثقة: 12 بأولوية P1 و12 بأولوية P2**. أُثبتت 13 مشكلة باختبارات إضافية، موزعة على 17 حالة للخادم و4 حالات لعقود الويب. بقية المشكلات مثبتة بتتبع الكود والعقود المشار إليها، ولم تُنفذ لها جلسات متصفح كاملة.

- **P1:** خلل أمني، أو تأثير مباشر على المخزون/الحسابات، أو تعطيل مسار أساسي.
- **P2:** خلل وظيفي أو نقص في اكتمال الميزة يحتاج إصلاحًا مخططًا.
- تم استبعاد مشروع سطح المكتب واختباراته تمامًا، ولم يُبنَ ملف الحل الذي يتضمنهما.
- تمت مراجعة حالة الملفات الحالية، بما فيها تعديلات العمل الموجودة قبل بدء المراجعة، دون تعديلها.
- التغييرات الجديدة مقتصرة على هذا التقرير وأدلة إعادة الإنتاج. لم تُطبق إصلاحات على كود التطبيق.

## التحقق المنفذ

| التحقق | النتيجة |
|---|---|
| بناء مشاريع الخادم عبر مشروع اختبارات SecurityTests | ناجح |
| الاختبارات الأصلية، مع تفعيل SQL Server | 146 ناجحًا، صفر فشل، صفر تخطٍ |
| اختبارات الويب الأصلية | 5 ناجحة في ملفين |
| `npm run build` | ناجح: TypeScript وVite |
| `npm run lint` | ناجح |
| اختبارات البحث الإضافية للخادم | 17 حالة كشفت مخالفة السلوك المتوقع |
| اختبارات عقود الويب الإضافية | 4 حالات كشفت مخالفة العقد المتوقع |

استخدمت اختبارات SQL Server قواعد جديدة بأسماء `RetalModelTests_<guid>` وحذفتها عند الانتهاء. لم تُستخدم قاعدة بيانات التطبيق. اختبار حذف الملفات استخدم مجلدًا مؤقتًا وملفات وهمية فقط.

نجاح الاختبارات الأصلية لا يغطي الحالات الجديدة التي كشفها هذا التقرير. أُرشفت الاختبارات الكاشفة بصيغة `.txt` حتى لا تصبح اختبارات فاشلة ضمن التشغيل المعتاد للمشروع.

## خريطة الميزات والتغطية

في كل صف تمت مراجعة النماذج وعقود الطلب/الاستجابة وتهيئة البيانات ومسار الخدمة والمتحكم والمستهلك في الويب. التحقق الآلي المذكور يشير إلى الاختبارات المتاحة والحالات الكاشفة؛ لا يعني اختبار جميع احتمالات التشغيل أو جميع تفاعلات المتصفح.

| # | الميزة | المسار من النماذج إلى الويب | المشكلات المرتبطة |
|---|---|---|---|
| 1 | المصادقة وعزل المستأجرين والجلسة | `Tenant/ApplicationUser` ← مرشحات `AppDbContext` ← `AuthService` وJWT ← `AuthContext/Shell` | F01، F08، F09 |
| 2 | المستخدمون والأدوار والصلاحيات | Identity و`UserDtos` ← `UserService` ← `UsersController` ← `UsersView/PermissionsMatrixModal` | F21، F24 |
| 3 | المؤسسة والفروع | `Tenant/Branch/BranchPhone` ← المستودعات و`TenantService/BranchService` ← المتحكمات ← `OrganizationView/Topbar` | F01، F18، F20 |
| 4 | الأصناف والتصنيفات والوحدات والباركود والصور | `Product/Category/Unit/ProductBarCode/ProductImage` ← خرائط التحويل والمستودعات ← خدمات الكتالوج ← `CatalogView` | F14، وتأثير F15 على التعاملات المرتبطة |
| 5 | استيراد وتصدير الأصناف | عقود Excel ← استعلامات المرشحين ← `ProductExcelService` ← endpoints Excel ← `ProductImportModal` | F06، F07 |
| 6 | العملاء والتحصيل وكشف الحساب | `Customer/CustomerPhone/CustomerTransaction` ← DTOs والمستودع ← `CustomerService` ← `CustomersView` والنوافذ | F04، F05، F15، F17، F22 |
| 7 | الموردون والسداد وكشف الحساب | `Supplier/SupplierPhone/SupplierTransaction` ← DTOs والمستودع ← `SupplierService` ← `SuppliersView` ونوافذ الشراء | F04، F05، F10، F15، F16 |
| 8 | فواتير المبيعات | `SalesInvoice/Items` ← القيود والمستودع ← `SalesInvoiceService` ← API ← `SalesInvoiceModal/SalesView` | F03، F09، F14، F20 |
| 9 | مرتجعات المبيعات | `SalesReturn/Items` ← عقود التقييم ← `SalesReturnService` ← API ← `SalesReturnModal` | F02، F13 |
| 10 | فواتير المشتريات وتفاصيل العبوات | `PurchaseInvoice/Items/Breakdowns` ← عقود الكميات والتكلفة ← الخدمة ← API ← `PurchaseInvoiceModal` | F03، F10، F20 |
| 11 | أوامر الشراء والاستلام | `PurchaseOrder/Items` ← DTOs والمستودع ← `PurchaseOrderService` وإنشاء فاتورة الاستلام ← الويب | F10، F12 |
| 12 | مرتجعات المشتريات | `PurchaseReturn/Items` ← كميات الفاتورة وتقييمها ← `PurchaseReturnService` ← `PurchaseReturnModal` | F02، F10؛ الشاشة لا توفر ربط الفاتورة الأصلية أيضًا |
| 13 | المخازن والأرصدة والجرد والتحويل | `Warehouse/StorgeStock/ShowroomStock/StockAdjustment/StockTransfer` ← المستودعات والخدمات ← المتحكمات ← `StockView` | F11، F12، F23 |
| 14 | الخزائن والمصروفات والتسوية المالية | `Safe/SafeTransaction/Expense` ← `FinancialPosting/Safe/Expense/FinancialReconciliation` ← API ← `AccountingView` | F19، F20، F21 |
| 15 | نقاط البيع والأجهزة والورديات | `PosTerminal/PosDeviceCredential/CashSession` ← القيود وخدمات الورديات ← API ← `PosContext/PosView` | F08، F09، F19 |
| 16 | لوحة المؤشرات والتقارير | الفواتير والمرتجعات والأرصدة ← `DashboardSalesMetrics/DashboardService` ← `DashboardController` ← `DashboardView` | أثر F04 على إجمالي المديونيات، واختلاف نطاق الفرع في F20 |

## المشكلات ذات الأولوية P1

### F01 — استبدال شعار المنشأة يمكن أن يحذف ملف مستأجر آخر

**الميزة:** المؤسسة/الصور/عزل المستأجرين. **الإثبات:** اختبار SQL Server مع استدعاء المتحكم وخدمة الملفات.

`UpdateTenantDto.LogoUrl` يقبل مسارًا يرسله العميل. تحفظه `TenantService.UpdateAsync` دون التحقق من ملكية الملف. عند رفع شعار جديد، يحذف `TenantsController.UploadLogo` الشعار السابق؛ وتتحقق `FileUploadService.DeleteAsync` من أن الملف داخل `uploads` فقط، دون التحقق من مجلد المستأجر الحالي.

اختبار `Tenant_logo_replacement_must_not_delete_another_tenants_file` عيّن شعار المنشأة إلى مسار ملف وهمي في مجلد مستأجر آخر ثم رفع شعارًا جديدًا. **حُذف الملف الآخر بالفعل.** يلزم معرفة مسار الملف المستهدف وامتلاك صلاحية تعديل المنشأة.

**المواضع:** `RetalSystemAPI.Models.DTOs/Tenant/UpdateTenantDto.cs:41`، `RetalSystemAPI.Services/Tenant/Implementations/TenantService.cs:138`، `RetalSystemAPI/Controllers/Tenants/TenantsController.cs:183`، `RetalSystemAPI.Services/FileUpload/Implementations/FileUploadService.cs:164`.

**الإصلاح:** منع تعديل مسار الملف مباشرة أو قبول مرجع رفع مملوك للمستأجر، والتحقق من ملكية المسار داخل خدمة الحذف نفسها.

### F02 — قبول مرتجعات على فواتير مسودة لم تُرحّل

**الميزة:** مرتجعات المبيعات والمشتريات. **الإثبات:** حالتان على SQL Server.

الخدمتان تمنعان `Cancelled/Voided` وتسمحان بـ`Draft`. بذلك تُعامل بنود مسودة قديمة كأنها بضاعة بيعت أو اشتريت فعلًا، مع تنفيذ أثر مخزني ومالي. منع إنشاء مسودات جديدة لا يعالج المسودات الموجودة التي يدعم النظام قراءتها وتعديلها/إلغاءها.

**النتيجة الفعلية:** مرتجع مسودة بيع رفع رصيد الصالة من 0 إلى 1 وجعل رصيد العميل `-100`. ومرتجع مسودة شراء خفض المخزن من 10 إلى 9 وجعل رصيد المورد `-10`.

**المواضع:** `RetalSystemAPI.Services/Sales/Implementations/SalesReturnService.cs:137`، `RetalSystemAPI.Services/Purchase/Implementations/PurchaseReturnService.cs:148`.

**الإصلاح:** قبول المرتجعات على حالات الفواتير المرحلة فقط والتحقق من هذا الشرط قبل أي تقييم أو ترحيل.

### F03 — فواتير البيع والشراء تسمح باختلاف فرع المستودع عن فرع الفاتورة

**الميزة:** المبيعات والمشتريات والفروع. **الإثبات:** حالتان على SQL Server.

يُفحص وجود الفرع ووجود المستودع منفصلين، دون شرط `warehouse.BranchId == dto.BranchId`. القيود الحالية تتحقق من المستأجر، ولا تفرض تطابق الفرع.

**النتيجة الفعلية:** نجح إنشاء فاتورتي بيع وشراء على الفرع B مع خصم/إضافة المخزون في مستودع الفرع A. تُنسب الحركة المالية والتقارير إلى فرع مختلف عن حركة البضاعة. اختيار المستودعات في بعض نوافذ الويب غير مقيد بالفرع أيضًا.

**المواضع:** `RetalSystemAPI.Services/Sales/Implementations/SalesInvoiceService.cs:114`، `RetalSystemAPI.Services/Purchase/Implementations/PurchaseInvoiceService.cs:164`، `retalsystem.web/src/features/sales/components/SalesInvoiceModal.tsx:117`.

**الإصلاح:** فرض تطابق الفرع على الخادم وتصفية المستودعات وإعادة ضبط الاختيار عند تغيير الفرع في الويب.

### F04 — حذف عميل أو مورد ذي رصيد قائم يخفي الدين ويمنع تسويته عبر المسارات المعتادة

**الميزة:** العملاء والموردون ولوحة المؤشرات. **الإثبات:** حالتان على SQL Server.

`DeleteAsync` ينفذ الحذف المنطقي دون فحص الرصيد. بعدها لا تعثر خدمات التحصيل/السداد على الطرف بسبب المرشحات العامة، ويخرج رصيده من إجمالي الذمم الذي يجمع سجلات العملاء/الموردين الظاهرة.

**النتيجة الفعلية:** قُبل حذف عميل ومورد برصيد 100. لم يُسجل قيد تسوية للدين؛ أصبح الطرف مخفيًا.

**المواضع:** `RetalSystemAPI.Services/Customers/Implementations/CustomerService.cs:215`، `RetalSystemAPI.Services/Suppliers/Implementations/SupplierService.cs:173`، `RetalSystemAPI.Services/Dashboard/Implementations/DashboardService.cs:89`.

**الإصلاح:** منع حذف الأطراف ذات الأرصدة القائمة، واستخدام التعطيل مع إبقاء التسوية والسجل المالي متاحين.

### F05 — كشف الحساب يعيد رصيدًا تاريخيًا خاطئًا عند إدخال دفعة بتاريخ سابق

**الميزة:** كشوف العملاء والموردين. **الإثبات:** حالتان على SQL Server.

تسجل الدفعة `BalanceAfter` من الرصيد الحالي وقت الإدخال، بينما يختار `GetBalanceBeforeDateAsync` آخر حركة بحسب `TransactionDate` ويعيد رصيدها المخزن. هذان الترتيبان يختلفان مع الإدخال بأثر رجعي.

**النتيجة الفعلية:** حركة بقيمة +100 في يوم لاحق، ثم إدخال دفعة 20 بتاريخ أقدم. الرصيد قبل فترة تبدأ بين التاريخين يجب أن يكون `-20`؛ أعاد النظام **80** لكل من العميل والمورد.

**المواضع:** `RetalSystemAPI.DataAccess/Repositories/Implementations/Customers/CustomerRepository.cs:94`، `RetalSystemAPI.DataAccess/Repositories/Implementations/Suppliers/SupplierRepository.cs:94`، `RetalSystemAPI.Services/Customers/Implementations/CustomerService.cs:392`، `RetalSystemAPI.Services/Suppliers/Implementations/SupplierService.cs:313`.

**الإصلاح:** احتساب الرصيد التاريخي من الرصيد الافتتاحي وصافي الحركات قبل التاريخ، وتحديد سياسة واضحة لأرصدة الأسطر عند السماح بالتأريخ السابق.

### F06 — استيراد Excel يسند الأرصدة إلى مخزن من النوع الخطأ عند غياب النوع المطلوب

**الميزة:** استيراد الأصناف والمخزون. **الإثبات:** حالتان على SQL Server.

عند عدم وجود صالة أو مخزن تخزين، تستبدل الخدمة القائمة بكل المستودعات عبر `ListLookupAsync`، ثم تنشئ الأرصدة في أول عنصر دون تحقق نهائي من النوع.

**النتيجة الفعلية:** أُنشئ سجل `StorgeStock` داخل صالة عند غياب مخزن التخزين، وأُنشئ `ShowroomStock` داخل مخزن تخزين عند غياب الصالة. القيود المرجعية الحالية لا تمنع ذلك.

**الموضع:** `RetalSystemAPI.Services/Catalog/Implementations/ProductExcelService.cs:605`.

**الإصلاح:** رفض رصيد افتتاحي يحتاج نوعًا غير متاح، والتحقق من النوع والنشاط وملكية المستودع المحدد في خيارات الاستيراد. لا يكفي منع الحالة في `ProductService` لأن Excel يستخدم مسارًا مستقلًا.

### F07 — قيمة سعر غير رقمية في Excel تصبح صفرًا وتكتب فوق سعر الصنف

**الميزة:** استيراد وتحديث الأصناف. **الإثبات:** اختبار على SQL Server.

تُهمل نتيجة `decimal.TryParse` ويُستخدم الصفر الناتج. يفحص التحقق السعر السالب فقط، فيعتبر النص غير الرقمي قيمة صالحة.

**النتيجة الفعلية:** أدخل الاختبار `not-a-price` لصنف تكلفته 10. أعادت المعاينة `InvalidRows=0`، ونجح الاستيراد وأصبحت التكلفة **0**.

**المواضع:** `RetalSystemAPI.Services/Catalog/Implementations/ProductExcelService.cs:331` و`:784` و`:877`.

**الإصلاح:** التحقق من نجاح التحويل والحدود والدقة، وتمييز القيمة المفقودة عن الصفر، وتوحيد قواعد المعاينة والتنفيذ. ينطبق النمط نفسه على حقول رقمية أخرى تستعمل `TryParse` دون فحص النتيجة.

### F08 — اختصارات نقطة البيع تستمر في العمل أثناء قفل الشاشة

**الميزة:** الجلسة ونقطة البيع. **الإثبات:** تتبع مسار أحداث لوحة المفاتيح.

القفل يعرض نافذة فوق الشاشة دون إلغاء تركيب نقطة البيع. `PosView` يسجل `useHotkeys` بالإعداد الافتراضي `enabled=true`، و`completeSale` لا يفحص `isLocked`. خطاف الاختصارات ينفذ مفاتيح F حتى أثناء التركيز داخل حقل كلمة المرور.

**إعادة الإنتاج:** جهز سلة ووردية مفتوحة، اقفل الشاشة، ثم اضغط `F8` لضبط الدفع و`F10` لإتمام البيع. يصل المسار إلى إجراء البيع دون فك القفل.

**المواضع:** `retalsystem.web/src/shared/components/layout/Shell.tsx:122`، `retalsystem.web/src/shared/hooks/useHotkeys.ts:29`، `retalsystem.web/src/features/pos/PosView.tsx:90` و`:118`، `retalsystem.web/src/features/pos/context/PosContext.tsx:418`.

**الإصلاح:** تعطيل أحداث الخلفية عند القفل وإضافة حارس داخل إجراءات نقطة البيع نفسها.

### F09 — صلاحيات إتمام بيع POS ومنح الخصم مفروضة في الواجهة فقط

**الميزة:** الصلاحيات ونقطة البيع. **الإثبات:** تتبع المتحكم والخدمة والعميل.

تفحص الواجهة `Permissions.Pos.CreateSale` و`Permissions.Pos.ApplyDiscount`، لكنها ترسل العملية إلى endpoint فواتير البيع الذي يفحص `Permissions.SalesInvoices.Create` فقط. لا تفحص الخدمة صلاحيات POS عند قبول الفاتورة أو الخصم.

**الأثر:** مستخدم يحتفظ بصلاحية إنشاء فواتير المبيعات، مثل كاشير بعد سحب صلاحية خصم POS أو إتمامه، يمكنه تجاوز منع الواجهة بطلب API مباشر. كذلك يرفض endpoint مستخدمًا مُنح صلاحية POS دون صلاحية إنشاء الفواتير، رغم سماح الواجهة بالإجراء.

**المواضع:** `retalsystem.web/src/features/pos/context/PosContext.tsx:419` و`:424`، `RetalSystemAPI/Controllers/Sales/SalesInvoicesController.cs:150`، `RetalSystemAPI.Services/Sales/Implementations/SalesInvoiceService.cs:163`.

**الإصلاح:** تحديد عقد وصلاحيات واضحة لمسار POS وفرضها على الخادم. يجب ألا يعتمد التعرف على مسار POS على بادئة رقم فاتورة يستطيع العميل تغييرها.

### F10 — قوائم الموردين في نوافذ الشراء تُفرغ بسبب غياب `isActive` من الاستجابة

**الميزة:** الموردون والمشتريات وأوامر الشراء ومرتجعاتها. **الإثبات:** اختبار خدمة وتسلسل تحويل إلى JSON، مع تتبع المستهلكين.

النموذج يحتوي `IsActive`، لكن `SupplierSummaryDto` و`SupplierResponseDto` لا يحتويانه. يمرر `supplierService` البيانات كما هي، ثم تنفذ نوافذ الشراء `filter(s => s.isActive)`؛ فتستبعد كل الموردين لأن القيمة `undefined`.

**الأثر:** تكون قائمة الموردين فارغة في إنشاء فاتورة شراء وأمر شراء ومرتجع شراء، ويظهر المورد في الجدول كأنه معطل. هذه مشكلة عقد بين الطبقات، حتى عندما تكون البيانات في قاعدة SQL صحيحة.

**المواضع:** `RetalSystemAPI.Models.DTOs/Suppliers/SupplierSummaryDto.cs`، `RetalSystemAPI.Models.DTOs/Suppliers/SupplierResponseDto.cs`، `retalsystem.web/src/features/purchase/components/PurchaseInvoiceModal.tsx:208`، `PurchaseOrderModal.tsx:94`، `PurchaseReturnModal.tsx:81`.

**الإصلاح:** تضمين حالة النشاط في العقود وتوحيد تحويل الاستجابات في عميل الموردين.

### F14 — مسح باركود بديل في فاتورة المبيعات قد يخصم البديل الأول

**الميزة:** الأصناف وفواتير المبيعات. **الإثبات:** تتبع البحث وإضافة البند إلى الطلب.

يعيد البحث بالباركود تفاصيل المنتج بكل باركوداته. يستبدل `handleSelectProduct` الباركود الممسوح بـ`barCodes[0]`، ثم يرسل `handleAddItem` معرف هذا الباركود الأول. لا يُحفظ الباركود المطابق للمسح.

**إعادة الإنتاج:** منتج بباركود A وB، حيث A أول القائمة. امسح B وأضف البند في فاتورة من مخزن تخزين؛ يُرسل معرف A، فيخصم الخادم مخزون A أو يرفض البيع لنفاد A رغم وجود B.

**المواضع:** `RetalSystemAPI.Services/Catalog/Implementations/ProductService.cs:114`، `retalsystem.web/src/features/sales/components/SalesInvoiceModal.tsx:167` و`:237`.

**الإصلاح:** حفظ الباركود المطابق في حالة مستقلة وإرساله مع البند، كما يفعل مسار البحث في خدمة POS.

### F22 — تحرير العميل من ملخص القائمة قد يمحو بياناته ويغيّر نوعه

**الميزة:** العملاء. **الإثبات:** تتبع عقد القائمة وتهيئة النموذج وطلب التعديل.

يمرر `CustomersView` ملخص الصف مباشرة إلى نموذج التحرير دون جلب التفاصيل. ملخص العميل لا يتضمن البريد والعنوان، فيبدآن فارغين. نموذج التحرير لا يحفظ `Code` أو `Type` في الطلب أيضًا، بينما خدمة التحديث تستبدلهما بقيم DTO، ومنها `Code=null` و`Type=Retail` الافتراضي.

**الأثر:** بعد إكمال الحقول المطلوبة وإرسال تعديل صالح لاسم عميل جملة مثلًا، يمكن أن يفقد الكود والعنوان ويتحول إلى قطاعي. ويُطلب إدخال الهاتف مجددًا بسبب F15. هذا استبدال لبيانات لم يقصد المستخدم تغييرها.

**المواضع:** `retalsystem.web/src/features/customers/CustomersView.tsx:352`، `retalsystem.web/src/features/customers/components/CustomerFormModal.tsx:41` و`:92`، `RetalSystemAPI.Models.DTOs/Customers/UpdateCustomerDto.cs:31`، `RetalSystemAPI.Services/Customers/Implementations/CustomerService.cs:155`.

**الإصلاح:** تحميل تفاصيل العميل قبل التحرير، والحفاظ على الحقول التي يتطلبها عقد الاستبدال، أو استخدام عقد تعديل جزئي واضح.

## المشكلات ذات الأولوية P2

### F11 — الويب لا يرسل الباركود المطلوب للتحويل والجرد في مخزن التخزين

**الميزة:** التحويلات والتسويات الجردية. **الإثبات:** مطابقة الطلب الذي تبنيه النافذتان مع شروط الخدمتين؛ الاختبارات الأصلية تغطي رفض التحويل دون باركود.

`StockTransferModal` و`StockAdjustmentModal` يرسلان `productId` فقط، دون اختيار/إرسال `productBarCodeId`. يرفض الخادم التحويل من/إلى مخزن تخزين، والجرد داخل مخزن تخزين، لأن الرصيد فيه يُدار بالباركود.

**المواضع:** `retalsystem.web/src/features/stock/components/StockTransferModal.tsx:68`، `StockAdjustmentModal.tsx:73`، `RetalSystemAPI.Services/Warehouses/Implementations/StockTransferService.cs:440`، `StockAdjustmentService.cs:115`.

**الإصلاح:** توفير اختيار الباركود وتحميل تفاصيل الصنف وإرسال معرفه، مع عرض الأرصدة المناسبة لنوع المستودع.

### F12 — أوامر التحويل والشراء تُنشأ كمسودات ولا توفر شاشة الويب مسار تنفيذها

**الميزة:** التحويل المخزني وأوامر الشراء. **الإثبات:** تتبع حالات الخدمات وحصر استدعاءات عميل الويب.

إنشاء التحويل يعين `Draft`، وترحيل المخزون يحدث عند `UpdateStatusAsync(Completed)` فقط. الويب يستدعي الإنشاء ويعرض «تم التحويل» دون أي استدعاء لتحديث الحالة أو إجراء تنفيذ في القائمة. كذلك تُنشأ أوامر الشراء كمسودات دون إجراء ويب لاستلامها، رغم أن الخدمة تدعم الاستلام وإنشاء فاتورة الشراء.

**الأثر:** التحويل بين صالتين قد يُحفظ بنجاح دون نقل كمية واحدة، وتظل دورة أوامر الشراء غير مكتملة من الويب.

**المواضع:** `RetalSystemAPI.Services/Warehouses/Implementations/StockTransferService.cs:152` و`:234`، `retalsystem.web/src/features/stock/StockView.tsx:549`، `RetalSystemAPI.Services/Purchase/Implementations/PurchaseOrderService.cs:282`، `retalsystem.web/src/features/purchase/services/purchaseService.ts:65`.

**الإصلاح:** إظهار حالة المسودة بدقة وإضافة إجراءات تنفيذ/استلام بالصلاحيات المناسبة، أو تقديم إجراء خادم واضح للإنشاء والتنفيذ إذا كان هذا التدفق المطلوب.

### F13 — شاشة مرتجع المبيعات لا تدعم الفاتورة الأصلية المطلوبة للمستخدم العادي

**الميزة:** مرتجعات المبيعات. **الإثبات:** مطابقة payload وشروط الخدمة.

لا تعرض النافذة اختيار فاتورة أصلية ولا ترسل `originalInvoiceId`. ترفض الخدمة ذلك ما لم يمتلك المستخدم `ReturnWithoutInvoice`. لذلك يفشل إنشاء المرتجع لمستخدم لديه صلاحية الإنشاء المعتادة. ولا يستطيع المستخدم المخول ربط المرتجع بفاتورة للاستفادة من كمياتها وأسعارها وخصوماتها الأصلية.

**المواضع:** `retalsystem.web/src/features/sales/components/SalesReturnModal.tsx:158`، `RetalSystemAPI.Services/Sales/Implementations/SalesReturnService.cs:151`.

**الإصلاح:** اختيار الفاتورة وإرجاع بنودها وكمياتها المتاحة، وإظهار مسار «دون فاتورة» فقط لحامل صلاحيته. شاشة مرتجع المشتريات تحتاج دعم الربط بالفاتورة أيضًا لتطبيق التقييم التاريخي الذي توفره الخدمة.

### F15 — عقد الهاتف مختلف بين شاشات العملاء/الموردين والخادم

**الميزة:** العملاء والموردون والإضافة السريعة في POS. **الإثبات:** ثلاثة اختبارات عقود ويب.

ترسل الواجهة `phone` مفردًا، بينما يتوقع الخادم `phones: [{ phoneNumber, ... }]`. تُهمل الخاصية غير المعروفة وتنجح العملية دون حفظ الهاتف. وفي القراءة يعيد ملخص العميل `primaryPhone` بينما تعرض الواجهة `phone`، فتختفي الأرقام الموجودة أيضًا.

**المواضع:** `retalsystem.web/src/features/customers/services/customerService.ts:79`، `retalsystem.web/src/features/suppliers/services/supplierService.ts:74`، `RetalSystemAPI.Models.DTOs/Customers/CreateCustomerDto.cs`، `RetalSystemAPI.Models.DTOs/Suppliers/CreateSupplierDto.cs`، `RetalSystemAPI.Models.DTOs/Customers/CustomerSummaryDto.cs:37`.

**الإصلاح:** محولات صريحة لطلبات الأطراف واستجاباتها، وجلب التفاصيل عندما تتطلب الشاشة بيانات لا يوفرها الملخص.

### F16 — تعديل قائمة هواتف المورد لا يُحفظ حتى عند إرسال DTO صحيح

**الميزة:** الموردون. **الإثبات:** اختبار SQL Server مستقل عن الويب.

تعتمد الخدمة على AutoMapper دون ربط `Phones` بـ`SupplierPhones` أو معالجة المجموعة يدويًا. الاسم المختلف يمنع تحديث المجموعة.

**النتيجة الفعلية:** أُرسل تعديل الهاتف من `0911111111` إلى `0922222222`. نجحت الخدمة، وبقي الهاتف القديم في SQL Server.

**المواضع:** `RetalSystemAPI.Services/Suppliers/Implementations/SupplierService.cs:145`، `RetalSystemAPI.Models.DTOs/MappingProfiles/SupplierMappingProfile.cs:33`.

**الإصلاح:** تطبيق سياسة استبدال/تحديث الهواتف صراحة مع ضبط معرف المورد والحذف والتحقق.

### F17 — إرسال قائمة هواتف عميل فارغة لا يمحو الهواتف السابقة

**الميزة:** العملاء. **الإثبات:** اختبار SQL Server.

شرط `dto.Phones.Any()` يتجاوز معالجة المجموعة عندما تكون فارغة، فتبقى الأرقام السابقة رغم نجاح طلب الاستبدال.

**الموضع:** `RetalSystemAPI.Services/Customers/Implementations/CustomerService.cs:169`.

**الإصلاح:** التمييز بين عدم إرسال المجموعة وإرسال قائمة فارغة، وتنفيذ إزالة الأرقام عند طلب القائمة الفارغة.

### F18 — بعض إعدادات المؤسسة تُعرض وتُرسل بأسماء لا يقبلها الخادم

**الميزة:** إعدادات المؤسسة. **الإثبات:** اختبار عقد ويب.

تستخدم الواجهة `companyName/phone/email/currencySymbol/receiptHeader/receiptFooter`، بينما العقد الحقيقي يستخدم `commercialName/phoneNumber/contactEmail/defaultCurrencySymbol/invoiceHeaderNote/invoiceFooterNote`. عميل المؤسسة يمرر الكائن دون تحويل.

**النتيجة الفعلية:** بعد تحميل الاستجابة الفعلية ثم تعديل حقول الواجهة، بقيت القيم الأصلية تحت أسماء الخادم وأُرسلت التعديلات تحت أسماء إضافية يتجاهلها الخادم. قد تظهر رسالة حفظ ناجح دون حفظ التغيير.

**المواضع:** `retalsystem.web/src/features/organization/services/organizationService.ts:107`، `OrganizationView.tsx:252`، `RetalSystemAPI.Models.DTOs/Tenant/UpdateTenantDto.cs`.

**الإصلاح:** توحيد الأسماء أو تحويل صريح في اتجاهي القراءة والكتابة مع اختبار دورة تحميل/تعديل/حفظ.

### F19 — البيع بالبطاقة في POS لا يحدد حساب التحصيل

**الميزة:** نقطة البيع والخزائن. **الإثبات:** تتبع payload وحل الخزينة.

يرسل POS معرف الوردية للنقد فقط ولا يرسل `paymentSafeId` للبطاقة. تختار الخدمة خزينة تلقائيًا فقط عند وجود خزينة نشطة وحيدة في الفرع. الإعداد الطبيعي الذي يحتوي درج نقد وحساب بنك يجعل الدفع بالبطاقة يفشل بسبب تعدد الخزائن، دون وسيلة في الشاشة لتحديد الحساب. وإذا كانت الخزينة الوحيدة درجًا نقديًا فقد يضاف تحصيل البطاقة إليه.

**المواضع:** `retalsystem.web/src/features/pos/context/PosContext.tsx:462`، `RetalSystemAPI.Services/Accounting/Implementations/FinancialPostingService.cs:40`.

**الإصلاح:** تهيئة حساب تحصيل البطاقة لنقطة البيع أو اختياره وإرساله، والتحقق من توافق الحساب مع وسيلة الدفع.

### F20 — تغيير الفرع النشط لا يغيّر نطاق عدة قوائم رئيسية

**الميزة:** الفروع والمبيعات والمشتريات والمحاسبة. **الإثبات:** تتبع طلبات القوائم؛ لا يوجد مستهلك لـ`X-Branch-Id` في كود الخادم.

يحفظ محول الفرع الاختيار ويضيفه عميل HTTP في header. لكن قوائم المبيعات والمشتريات والخزائن لا ترسل `branchId` في query ولا تعيد التحميل استجابة لتغييره. متحكماتها تعتمد معامل query؛ أما header فلا يُطبق كفلتر عام. لوحة المؤشرات ترسل الفرع، فيصبح نطاقها مختلفًا عن القوائم.

**المواضع:** `retalsystem.web/src/shared/context/AuthContext.tsx:156`، `retalsystem.web/src/features/sales/SalesView.tsx:66`، `retalsystem.web/src/features/purchase/PurchaseView.tsx:81`، `retalsystem.web/src/features/accounting/AccountingView.tsx:96`.

**الإصلاح:** تعريف نطاق الفرع المتوقع لكل شاشة، وتمريره في الاستعلام وإضافته إلى اعتماديات إعادة التحميل. هذه مشكلة تصفية داخل المستأجر، وليست دليلًا على تسريب بين المستأجرين.

### F21 — صلاحية شاشة رئيسية تمنع الوصول إلى ميزات فرعية مصرح بها

**الميزة:** التنقل والصلاحيات. **الإثبات:** خريطة عرض التطبيق وSidebar.

يربط `App` كل مجموعة بصلاحية واحدة: المحاسبة بـ`Safes.View` والمبيعات بـ`SalesInvoices.View` والمخزون بـ`Stock.View` والمستخدمين بـ`Users.View`. مستخدم لديه `Expenses.View` فقط أو `SalesReturns.View` فقط أو `Roles.View` فقط لا يصل إلى الميزة المصرح بها، رغم أن endpoints الخاصة بها تقبل صلاحياتها المستقلة.

**المواضع:** `retalsystem.web/src/App.tsx:19` و`:44`، `retalsystem.web/src/shared/components/layout/Sidebar.tsx:89`.

**الإصلاح:** إتاحة المجموعة عند وجود صلاحية لأي تبويب فيها، واختيار أول تبويب مسموح، ومنع طلب بيانات تبويبات غير مصرح بها.

### F23 — طلب قراءة أرصدة غير مرقم ينشئ سجلات من نوع مخالف للمستودع

**الميزة:** المخزون وواجهات API. **الإثبات:** حالتان على SQL Server.

تُهيئ خدمتا القراءة الأرصدة المفقودة وتحفظانها، دون التحقق من وجود المستودع ونوعه قبل الإنشاء. تمرير معرف صالة لمسار `stock/storge/{id}` أنشأ سجل تخزين فيها، والعكس صحيح. المسار المرقم الذي يستخدمه الويب لا ينفذ التهيئة نفسها؛ الخلل موجود في endpoints القراءة غير المرقمة.

**المواضع:** `RetalSystemAPI.Services/Warehouses/Implementations/StockService.cs:56` و`:203`، `RetalSystemAPI/Controllers/Warehouses/StockController.cs:46` و`:144`.

**الإصلاح:** جعل القراءة بلا كتابة، أو فصل التهيئة في إجراء كتابة والتحقق من نوع المستودع. إضافة قيود مناسبة لمنع تخزين أرصدة في نوع مخالف توفر حماية إضافية.

### F24 — مصفوفة صلاحيات الدور ترسل الاسم بدل المعرف

**الميزة:** المستخدمون والأدوار. **الإثبات:** تتبع اختيار الدور حتى استعلام Identity.

تحتفظ الشاشة بـ`r.name` فقط، وتمرره إلى `getRolePermissions/updateRolePermissions` كأنه `roleId`. تستخدم الخدمة `FindByIdAsync`، بينما أدوار Identity العادية تملك معرفات مستقلة عن أسماء مثل `Cashier`.

**الأثر:** يفشل تحميل المصفوفة وحفظها برسالة أن الدور غير موجود، حتى مع صلاحيات مسؤول المنصة. فحص `roleName === 'Admin'` في النافذة لا يحل مشكلة تمرير المعرف.

**المواضع:** `retalsystem.web/src/features/users/UsersView.tsx:348`، `retalsystem.web/src/features/users/components/PermissionsMatrixModal.tsx:32` و`:59`، `RetalSystemAPI.Services/Users/Implementations/UserService.cs:710` و`:760`.

**الإصلاح:** تمرير `roleId` مستقلًا عن الاسم والتسمية، واستخدامه في المسار والطلب.

## الأدلة وإعادة التشغيل

| الملف | المحتوى |
|---|---|
| [FeatureAuditReproductionTests.cs.txt](feature-audit-evidence/FeatureAuditReproductionTests.cs.txt) | 17 حالة كاشفة للخادم؛ 16 منها تستخدم SQL Server وواحدة تتحقق من عقد JSON للمورد |
| [feature-audit-2026-10-18.trx](feature-audit-evidence/feature-audit-2026-10-18.trx) | نتائج أول 16 حالة بعد عزل سياق التتبع بصورة صحيحة |
| [feature-audit-logo-2026-10-18.trx](feature-audit-evidence/feature-audit-logo-2026-10-18.trx) | نتيجة اختبار عزل ملفات الشعارات |
| [FeatureAuditReproductions.test.ts.txt](feature-audit-evidence/FeatureAuditReproductions.test.ts.txt) | 4 حالات عقود ويب، تستخدم نقل HTTP بديلًا دون الاتصال بالخادم |
| [web-contracts-2026-10-18.json](feature-audit-evidence/web-contracts-2026-10-18.json) | تفاصيل نتائج حالات الويب الأربع |
| [baseline-2026-10-18.trx](feature-audit-evidence/baseline-2026-10-18.trx) | نتيجة إعادة تشغيل الاختبارات الأصلية بعد أرشفة الحالات الكاشفة |

لإعادة اختبارات الخادم الكاشفة، انسخ ملف `.cs.txt` إلى `tests/RetalSystemAPI.SecurityTests/FeatureAuditReproductionTests.cs`، واضبط اتصال SQL Server اختباريًا يسمح بإنشاء قواعد مؤقتة، ثم نفذ:

```powershell
$env:RETAL_TEST_SQLSERVER = 'Server=localhost;Integrated Security=true;TrustServerCertificate=true;Connection Timeout=10'
dotnet test tests/RetalSystemAPI.SecurityTests/RetalSystemAPI.SecurityTests.csproj --filter FullyQualifiedName~FeatureAuditReproductionTests
```

ولحالات الويب، انسخ ملف `.test.ts.txt` إلى `retalsystem.web/src/FeatureAuditReproductions.test.ts` ثم نفذ من مجلد الويب:

```powershell
npm test -- src/FeatureAuditReproductions.test.ts
```

هذه الحالات تصف السلوك الصحيح المتوقع؛ فشلها في النسخة التي تمت مراجعتها هو دليل المشكلة. بعد الإصلاح ينبغي أن تنجح، مع تحويل الحالات المناسبة إلى اختبارات انحدار دائمة.

## ترتيب الإصلاح وحدود الاستنتاج

1. عزل ملفات المستأجرين، ثم حماية القفل وفرض صلاحيات POS على الخادم.
2. سلامة الترحيل: المسودات، تطابق الفرع، الأرصدة التاريخية، ومنع إخفاء الديون بالحذف.
3. الاستيراد: فحص القيم الرقمية ونوع المستودع، ثم منع الكتابة من مسارات القراءة.
4. العقود التي تعطل العمل: نشاط المورد، الهواتف، ربط المرتجع بالفاتورة، وحفظ بيانات العميل وإعدادات المؤسسة.
5. إكمال دورة التحويل والاستلام، وتوجيه تحصيل البطاقة، وصلاحيات التنقل ومعرف الدور.

لم تُنفذ اختبارات تحميل أو أجهزة طباعة فعلية أو جلسة متصفح لكل تدفق، ولم تُراجع إعدادات إنتاج أو بيانات حقيقية. لذلك لا يُستنتج من هذا التقرير خلو بقية المسارات من العيوب. إثباتات SQL تختبر الخدمات والمستودعات والقيود الحقيقية؛ اختبار الشعار يستدعي المتحكم مباشرة، واختبارات الويب تتحقق من عقود العميل. أُبقيت هذه الحدود منفصلة عن نتائج الاختبارات الأصلية الناجحة.
