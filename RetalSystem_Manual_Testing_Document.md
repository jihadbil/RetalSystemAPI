# وثيقة خطة وتنفيذ الاختبار اليدوي الشامل (Manual Testing Document)
## لنظام إدارة المبيعات والمخازن المتكامل (Retal System Pro / Emerald Pro)

---

### 📋 معلومات الفحص والبيئة التجريبية (Test Environment Setup)
- **اسم النظام**: Retal System API & Desktop Management System
- **تاريخ إعداد الوثيقة**: 2026-09-07
- **بيئة الاختبار**: Localhost / Development & Desktop UI (WPF)
- **اسم المختبر**: [اسم الفاحص: ___________________________ ]
- **تاريخ بدء الاختبار**: [تاريخ البدء: ___________________________ ]
- **تاريخ انتهاء الاختبار**: [تاريخ الانتهاء: ___________________________ ]
- **النتيجة الإجمالية**: [ ] معتمد وجاهز للنشر (Passed)  |  [ ] يحتاج إصلاحات ومراجعة (Failed)

---

## إرشادات للمختبر:
1. يرجى اتباع خطوات كل سيناريو اختبار بدقة ومقارنتها مع **النتيجة المتوقعة**.
2. حدد حالة الاختبار بوضع علامة (✓) في أحد الخيارين: `[ ] ناجح` أو `[ ] فاشل`.
3. اكتب أي ملاحظات، أرقام فواتير تم إنشاؤها، أو رسائل خطأ ظهرت لك يدوياً داخل المساحة المخصصة بين القوسين `[ ]`.

---

# القسم الأول: الأمان، تسجيل الدخول، وإدارة المستخدمين (Authentication & Users)

### TC-AUTH-001: تسجيل الدخول ببيانات صحيحة (Happy Path)
- **الهدف**: التحقق من قدرة المستخدم على تسجيل الدخول للنظام بالبيانات الصحيحة وتخزين الجلسة بنجاح.
- **الشروط المسبقة**: وجود مستخدم مسجل مسبقاً (مثل: Admin).
- **خطوات الاختبار**:
  1. تشغيل التطبيق والوصول إلى شاشة تسجيل الدخول `LoginView`.
  2. إدخال اسم المستخدم وكلمة المرور الصحيحة.
  3. الضغط على زر "تسجيل الدخول".
- **النتيجة المتوقعة**: يتم التحقق بنجاح، إغلاق نافذة الدخول، فتح الشاشة الرئيسية (ShellWindow) وعرض اسم المستخدم والفرع الحالي.
- **حالة الاختبار**: [✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [                                                                                ]

---

### TC-AUTH-002: تسجيل الدخول ببيانات غير صحيحة (Validation & Security)
- **الهدف**: التحقق من منع الدخول ببيانات خاطئة وإظهار رسالة تحذير واضحة دون انهيار التطبيق.
- **الشروط المسبقة**: فتح شاشة تسجيل الدخول.
- **خطوات الاختبار**:
  1. إدخال اسم مستخدم غير موجود أو كلمة مرور غير صحيحة.
  2. الضغط على زر "تسجيل الدخول".
- **النتيجة المتوقعة**: تظهر رسالة خطأ واضحة باللغة العربية تفيد بأن "اسم المستخدم أو كلمة المرور غير صحيحة"، ويبقى المستخدم في شاشة الدخول مع تفريغ حقل كلمة المرور.
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [      ناجح لكن شاشة تسجيل الدخول تختفي ثم تظهر من جديد و لا تظهر اي رسالة حول ان هناك خطا في البيانات ]

---

### TC-AUTH-003: إنشاء مستخدم جديد وتعيين الصلاحيات والفرع
- **الهدف**: التحقق من إضافة مستخدم جديد للنظام وربطه بفرع محدد وتعيين دور (كاشير/مدير).
- **الشروط المسبقة**: تسجيل الدخول كمسؤول (Admin).
- **خطوات الاختبار**:
  1. الانتقال إلى "الإدارة والنظام" -> "إدارة المستخدمين" -> الضغط على "إضافة مستخدم".
  2. إدخال (الاسم الكامل، اسم المستخدم، البريد، رقم الهاتف، كلمة المرور، تحديد الدور، واختيار الفرع).
  3. حفظ البيانات.
- **النتيجة المتوقعة**: يتم حفظ المستخدم وظهوره فوراً في جدول المستخدمين، وعند تسجيل الدخول بحسابه يتم توجيهه للفرع المحدد له.
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [ اولا لا يوجد صلاحيات لتعيينها و قد عينته كاشير مع  دالك كان قادرا على الدخول الي ك لانظام و لا اعتقد ان التحويل للفرع يعمل بشكل صحيح  ]

---

### TC-AUTH-004: إعادة تعيين كلمة المرور (Reset Password)
- **الهدف**: التحقق من إمكانية تغيير كلمة المرور لمستخدم بواسطة المدير.
- **الشروط المسبقة**: وجود مستخدم في القائمة.
- **خطوات الاختبار**:
  1. اختيار مستخدم من قائمة المستخدمين والضغط على "إعادة تعيين كلمة المرور".
  2. إدخال كلمة المرور الجديدة وتأكيدها.
  3. تسجيل الخروج ومحاولة الدخول بكلمة المرور الجديدة.
- **النتيجة المتوقعة**: يتم قبول كلمة المرور الجديدة ويتمكن المستخدم من الدخول بها بنجاح مع رفض كلمة المرور القديمة.
- **حالة الاختبار**: [ ] ناجح    [ ✓] فاشل
- **ملاحظات الفاحص**: [لا يوجد خيار لاعادة تعيين كلمة المرور اساسا]

---

### TC-AUTH-005: تسجيل الخروج (Logout Flow)
- **الهدف**: التأكد من إنهاء جلسة المستخدم الحالية تماماً عند تسجيل الخروج.
- **الشروط المسبقة**: التطبيق مفتوح ومسجل الدخول.
- **خطوات الاختبار**:
  1. الضغط على زر "تسجيل الخروج" في أسفل القائمة الجانبية.
  2. تأكيد الخروج إذا ظهر مربع حوار تأكيدي.
- **النتيجة المتوقعة**: يتم إغلاق الواجهة الرئيسية والعودة لنافذة تسجيل الدخول وحذف بيانات الجلسة المؤقتة.
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: []

---

# القسم الثاني: إعدادات المؤسسة والفروع (Tenants & Branches)

### TC-BRN-001: إضافة فرع جديد وأرقام هواتفه
- **الهدف**: التحقق من القدرة على إنشاء فرع جديد للمؤسسة مع أرقام اتصال متعددة.
- **الشروط المسبقة**: الدخول بحساب مدير النظام.
- **خطوات الاختبار**:
  1. التوجه إلى "الإدارة والنظام" -> "الفروع".
  2. النقر على "إضافة فرع جديد".
  3. تعبئة الاسم (مثال: فرع الشرقية)، العنوان، وإضافة رقمين للهاتف (أساسي وثانوي).
  4. الضغط على حفظ.
- **النتيجة المتوقعة**: إنشاء الفرع بنجاح وظهوره في القوائم المنسدلة لاختيار الفروع في المبيعات والمخازن.
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [نجح في اضافة الفرع لكنه فشل في اضافة رقم الهاتف]

---

### TC-BRN-002: تعديل بيانات المؤسسة (Tenant Profile)
- **الهدف**: التأكد من حفظ وتحديث بيانات المؤسسة التجارية وتأثيرها على ترويسة الفواتير.
- **الشروط المسبقة**: فتح شاشة "الملف الشخصي للمؤسسة".
- **خطوات الاختبار**:
  1. تعديل الاسم التجاري، الرقم الضريبي، وشعار المؤسسة.
  2. الضغط على حفظ.
- **النتيجة المتوقعة**: يتم حفظ البيانات وإظهار إشعار نجاح، مع انعكاس الاسم الجديد في شريط النظام العلوي.
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [ناجح بشكل كامل لكن ليس لدي عمليات طباعة الفاتورة الأن لاني لم افعلها بعد]

---

# القسم الثالث: دليل المنتجات والكتالوج (Catalog: Categories, Units & Products)

### TC-CAT-001: إدارة التصنيفات (Categories CRUD)
- **الهدف**: إضافة وتعديل وحذف تصنيف أصناف.
- **الشروط المسبقة**: الانتقال لشاشة "التصنيفات".
- **خطوات الاختبار**:
  1. إضافة تصنيف رئيسي جديد (مثال: مشروبات ساخنة).
  2. تعديل اسم التصنيف إلى (مشروبات ساخنة وعصائر).
  3. محاولة حذف تصنيف فارغ، ثم تصنيف مرتبط بمنتجات.
- **النتيجة المتوقعة**: يتم الإضافة والتعديل بنجاح، ويسمح النظام بحذف التصنيف الفارغ بينما يرفض أو يحذر عند حذف تصنيف يحتوي على أصناف.
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [نجاح في اضافة تصنيف نجاح حذف تصنيف فارغ نجاح عدم حذف تصنيف مرتبط بمنتجات فشل في تعديل بيانات تصنيف
مع رسالة خطا il: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      System.InvalidOperationException: The instance of entity type 'Category' cannot be tracked because another instance with the same key value for {'Id'} is already being tracked. When attaching existing entities, ensure that only one entity instance with a given key value is attached. Consider using 'DbContextOptionsBuilder.EnableSensitiveDataLogging' to see the conflicting key values.
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IdentityMap`1.ThrowIdentityConflict(InternalEntityEntry entry)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IdentityMap`1.Add(TKey key, InternalEntityEntry entry, Boolean updateDuplicate)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IdentityMap`1.Add(TKey key, InternalEntityEntry entry)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IdentityMap`1.Add(InternalEntityEntry entry)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.StartTracking(InternalEntityEntry entry)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.InternalEntityEntry.OnStateChanging(EntityState newState)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.InternalEntryBase.SetEntityState(EntityState oldState, EntityState newState, Boolean acceptChanges, Boolean modifyProperties)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.InternalEntityEntry.SetEntityState(EntityState entityState, Boolean acceptChanges, Boolean modifyProperties, Nullable`1 forceStateWhenUnknownKey, Nullable`1 fallbackState)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityGraphAttacher.PaintAction(EntityEntryGraphNode`1 node)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityEntryGraphIterator.TraverseGraph[TState](EntityEntryGraphNode`1 node, Func`2 handleNode)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityEntryGraphIterator.TraverseGraph[TState](EntityEntryGraphNode`1 node, Func`2 handleNode)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityGraphAttacher.AttachGraph(InternalEntityEntry rootEntry, EntityState targetState, EntityState storeGeneratedWithKeySetTargetState, Boolean forceStateWhenUnknownKey)
         at Microsoft.EntityFrameworkCore.Internal.InternalDbSet`1.SetEntityState(InternalEntityEntry entry, EntityState entityState)
         at Microsoft.EntityFrameworkCore.Internal.InternalDbSet`1.Attach(TEntity entity)
         at RetalSystemAPI.DataAccess.Repositories.Implementations.Repository`1.Update(T entity) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI.DataAccess\Repositories\Implementations\Repository.cs:line 243
         at RetalSystemAPI.Services.Catalog.Implementations.CategoryService.UpdateAsync(Guid id, UpdateCategoryDto dto, CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI.Services\Catalog\Implementations\CategoryService.cs:line 142
         at RetalSystemAPI.Controllers.Catalog.CategoriesController.Update(Guid id, UpdateCategoryDto dto, CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI\Controllers\Catalog\CategoriesController.cs:line 98
         at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Logged|12_1(ControllerActionInvoker invoker)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
         at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

]

---

### TC-UNIT-001: تعريف وحدات القياس ومعاملات التحويل (Units & Conversions)
- **الهدف**: إنشاء وحدات قياس (حبة، درزن، كرتون) وتحديد العلاقات الرياضية بينها.
- **الشروط المسبقة**: فتح شاشة "وحدات القياس".
- **خطوات الاختبار**:
  1. إضافة وحدة رئيسية "حبة" (معامل التحويل = 1).
  2. إضافة وحدة مجمعة "كرتون" (معامل التحويل = 24 بالنسبة للحبة).
  3. حفظ الوحدات.
- **النتيجة المتوقعة**: حفظ الوحدات وظهورها في شاشة بطاقة الصنف لاختيار الوحدة الكبرى والوحدة الصغرى.
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [نجاح اضافة وحدة نجاح حدف وحدة نجاح تعديل وحدة ]

---

### TC-PRD-001: إنشاء بطاقة صنف متكاملة مع نكهات وباركودات متعددة
- **الهدف**: اختبار تسجيل منتج جديد يحوي تفاصيل متعددة (باركود دولي، نكهات/تنوعات، وسعر شراء وسعر بيع).
- **الشروط المسبقة**: توفر تصنيف ووحدة قياس.
- **خطوات الاختبار**:
  1. الدخول إلى "المنتجات والأصناف" -> "إضافة صنف جديد".
  2. إدخال الاسم: "عصير طبيعي 1 لتر"، الكود: "JUICE-01"، اختيار التصنيف والوحدة الأساسية.
  3. إضافة تفاصيل الباركودات/النكهات في تبويب الباركودات:
     - نكهة الفراولة: باركود `6281001`
     - نكهة البرتقال: باركود `6281002`
     - نكهة المانجو: باركود `6281003`
  4. إدخال سعر التكلفة التقديري (10 ريال) وسعر البيع (15 ريال).
  5. حفظ الصنف.
- **النتيجة المتوقعة**: يتم إنشاء المنتج وتوليد معرّفات الباركود لكل نكهة بنجاح، وظهوره في جدول الأصناف.
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [نجاح كامل لكن لا يمكن اضافة الصور و ربطها بالباركود الا بعد اضافة و الحفظ في قاعدة البيانات لان القئمة المنسدلة لا تعرض الا من قاعدة البيانات]

---

### TC-PRD-002: التحقق من منع تكرار الباركود (Unique Barcode Validation)
- **الهدف**: التأكد من أن النظام يرفض حفظ صنف بباركود موجود مسبقاً في النظام.
- **الشروط المسبقة**: وجود صنف مسجل بباركود `6281001`.
- **خطوات الاختبار**:
  1. محاولة إضافة صنف جديد أو نكهة جديدة باستخدام نفس الباركود `6281001`.
  2. الضغط على حفظ.
- **النتيجة المتوقعة**: ظهور رسالة خطأ صريحة تشير إلى أن "الباركود مستخدم بالفعل لصنف آخر"، مع رفض الحفظ.
- **حالة الاختبار**: [✓ ] ناجح    [] فاشل
- **ملاحظات الفاحص**: [يطي الخطا التالي بدون توضيح التفاصيل للمستخدم
fail: Microsoft.EntityFrameworkCore.Update[10000]
      An exception occurred in the database while saving changes for context type 'RetalSystemAPI.DataAccess.Context.AppDbContext'.
      Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
       ---> Microsoft.Data.SqlClient.SqlException (0x80131904): Cannot insert duplicate key row in object 'dbo.ProductBarCodes' with unique index 'IX_ProductBarCodes_TenantId_BarCode'. The duplicate key value is (0343e41d-bc65-4a4e-8a6f-78f99e688f40, 20).
         at Microsoft.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
         at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
         at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, SqlCommand command, Boolean callerHasConnectionLock, Boolean asyncClose)
         at Microsoft.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)
         at Microsoft.Data.SqlClient.SqlDataReader.TryHasMoreRows(Boolean& moreRows)
         at Microsoft.Data.SqlClient.SqlDataReader.TryReadInternal(Boolean setTimeout, Boolean& more)
         at Microsoft.Data.SqlClient.SqlDataReader.ReadAsyncExecute(Task task, Object state)
         at Microsoft.Data.SqlClient.SqlDataReader.InvokeAsyncCall[T](SqlDataReaderBaseAsyncCallContext`1 context)
      --- End of stack trace from previous location ---
         at Microsoft.EntityFrameworkCore.Update.AffectedCountModificationCommandBatch.ConsumeResultSetAsync(Int32 startCommandIndex, RelationalDataReader reader, CancellationToken cancellationToken)
      ClientConnectionId:ef62f063-880e-4214-92f7-32e6af7fc855
      Error Number:2601,State:1,Class:14
         --- End of inner exception stack trace ---
         at Microsoft.EntityFrameworkCore.Update.AffectedCountModificationCommandBatch.ConsumeResultSetAsync(Int32 startCommandIndex, RelationalDataReader reader, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.AffectedCountModificationCommandBatch.ConsumeAsync(RelationalDataReader reader, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.SqlServer.Update.Internal.SqlServerModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
      Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
       ---> Microsoft.Data.SqlClient.SqlException (0x80131904): Cannot insert duplicate key row in object 'dbo.ProductBarCodes' with unique index 'IX_ProductBarCodes_TenantId_BarCode'. The duplicate key value is (0343e41d-bc65-4a4e-8a6f-78f99e688f40, 20).
         at Microsoft.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
         at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
         at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, SqlCommand command, Boolean callerHasConnectionLock, Boolean asyncClose)
         at Microsoft.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)
         at Microsoft.Data.SqlClient.SqlDataReader.TryHasMoreRows(Boolean& moreRows)
         at Microsoft.Data.SqlClient.SqlDataReader.TryReadInternal(Boolean setTimeout, Boolean& more)
         at Microsoft.Data.SqlClient.SqlDataReader.ReadAsyncExecute(Task task, Object state)
         at Microsoft.Data.SqlClient.SqlDataReader.InvokeAsyncCall[T](SqlDataReaderBaseAsyncCallContext`1 context)
      --- End of stack trace from previous location ---
         at Microsoft.EntityFrameworkCore.Update.AffectedCountModificationCommandBatch.ConsumeResultSetAsync(Int32 startCommandIndex, RelationalDataReader reader, CancellationToken cancellationToken)
      ClientConnectionId:ef62f063-880e-4214-92f7-32e6af7fc855
      Error Number:2601,State:1,Class:14
         --- End of inner exception stack trace ---
         at Microsoft.EntityFrameworkCore.Update.AffectedCountModificationCommandBatch.ConsumeResultSetAsync(Int32 startCommandIndex, RelationalDataReader reader, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.AffectedCountModificationCommandBatch.ConsumeAsync(RelationalDataReader reader, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.SqlServer.Update.Internal.SqlServerModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes. See the inner exception for details.
       ---> Microsoft.Data.SqlClient.SqlException (0x80131904): Cannot insert duplicate key row in object 'dbo.ProductBarCodes' with unique index 'IX_ProductBarCodes_TenantId_BarCode'. The duplicate key value is (0343e41d-bc65-4a4e-8a6f-78f99e688f40, 20).
         at Microsoft.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
         at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)
         at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, SqlCommand command, Boolean callerHasConnectionLock, Boolean asyncClose)
         at Microsoft.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)
         at Microsoft.Data.SqlClient.SqlDataReader.TryHasMoreRows(Boolean& moreRows)
         at Microsoft.Data.SqlClient.SqlDataReader.TryReadInternal(Boolean setTimeout, Boolean& more)
         at Microsoft.Data.SqlClient.SqlDataReader.ReadAsyncExecute(Task task, Object state)
         at Microsoft.Data.SqlClient.SqlDataReader.InvokeAsyncCall[T](SqlDataReaderBaseAsyncCallContext`1 context)
      --- End of stack trace from previous location ---
         at Microsoft.EntityFrameworkCore.Update.AffectedCountModificationCommandBatch.ConsumeResultSetAsync(Int32 startCommandIndex, RelationalDataReader reader, CancellationToken cancellationToken)
      ClientConnectionId:ef62f063-880e-4214-92f7-32e6af7fc855
      Error Number:2601,State:1,Class:14
         --- End of inner exception stack trace ---
         at Microsoft.EntityFrameworkCore.Update.AffectedCountModificationCommandBatch.ConsumeResultSetAsync(Int32 startCommandIndex, RelationalDataReader reader, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.AffectedCountModificationCommandBatch.ConsumeAsync(RelationalDataReader reader, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.ReaderModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.SqlServer.Update.Internal.SqlServerModificationCommandBatch.ExecuteAsync(IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Update.Internal.BatchExecutor.ExecuteAsync(IEnumerable`1 commandBatches, IRelationalConnection connection, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.Storage.RelationalDatabase.SaveChangesAsync(IList`1 entries, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(IList`1 entriesToSave, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.SaveChangesAsync(StateManager stateManager, Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.SqlServer.Storage.Internal.SqlServerExecutionStrategy.ExecuteAsync[TState,TResult](TState state, Func`4 operation, Func`4 verifySucceeded, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
         at Microsoft.EntityFrameworkCore.DbContext.SaveChangesAsync(Boolean acceptAllChangesOnSuccess, CancellationToken cancellationToken)
         at RetalSystemAPI.DataAccess.Context.AppDbContext.SaveChangesAsync(CancellationToken cancellationToken) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI.DataAccess\Context\AppDbContext.cs:line 194
         at RetalSystemAPI.DataAccess.Repositories.Implementations.UnitOfWork.SaveChangesAsync(CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI.DataAccess\Repositories\Implementations\UnitOfWork.cs:line 166
         at RetalSystemAPI.Services.Catalog.Implementations.ProductService.CreateAsync(CreateProductDto dto, CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI.Services\Catalog\Implementations\ProductService.cs:line 138
         at RetalSystemAPI.Controllers.Catalog.ProductsController.Create(CreateProductDto dto, CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI\Controllers\Catalog\ProductsController.cs:line 89
         at Microsoft.AspNetCore.Mvc.Infrastructure.ActionMethodExecutor.TaskOfIActionResultExecutor.Execute(ActionContext actionContext, IActionResultTypeMapper mapper, ObjectMethodExecutor executor, Object controller, Object[] arguments)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeActionMethodAsync>g__Logged|12_1(ControllerActionInvoker invoker)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeNextActionFilterAsync>g__Awaited|10_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Rethrow(ActionExecutedContextSealed context)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.Next(State& next, Scope& scope, Object& state, Boolean& isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ControllerActionInvoker.<InvokeInnerFilterAsync>g__Awaited|13_0(ControllerActionInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeFilterPipelineAsync>g__Awaited|20_0(ResourceInvoker invoker, Task lastTask, State next, Scope scope, Object state, Boolean isCompleted)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
         at Microsoft.AspNetCore.Mvc.Infrastructure.ResourceInvoker.<InvokeAsync>g__Logged|17_1(ResourceInvoker invoker)
         at Microsoft.AspNetCore.Authorization.AuthorizationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Authentication.AuthenticationMiddleware.Invoke(HttpContext context)
         at Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddlewareImpl.Invoke(HttpContext context)

]

---

### TC-PRD-003: استيراد الأصناف عبر معالج الإكسل (Product Excel Import)
- **الهدف**: فحص معالج استيراد بيانات الأصناف بكميات كبيرة من ملف Excel.
- **الشروط المسبقة**: تجهيز ملف إكسل يحتوي على أعمدة (الاسم، الباركود، التصنيف، الوحدة، الأسعار).
- **خطوات الاختبار**:
  1. في شاشة المنتجات، الضغط على "استيراد من إكسل".
  2. اختيار الملف وتطابق الأعمدة (Mapping) ومراجعة المعاينة.
  3. تأكيد عملية الاستيراد.
- **النتيجة المتوقعة**: تتم قراءة الأسطر وإدراج الأصناف بنجاح في قاعدة البيانات مع عرض تقرير بعدد الأصناف المستوردة والأخطاء إن وجدت.
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [نجاح لكن لسبب ما النظام اصبح بطيء يبدو ان هناك حاجة لتحسين الادء]

---

# القسم الرابع: إدارة المخازن، الأرصدة، والتحويلات (Warehouses & Stock)

### TC-WH-001: إنشاء وتصنيف المخازن (مستودع رئيسي Storage مقابل صالة عرض Showroom)
- **الهدف**: التحقق من إنشاء نوعي المخازن الأساسيين في بنية النظام.
- **الشروط المسبقة**: شاشة "المخازن وصالات العرض".
- **خطوات الاختبار**:
  1. إضافة موقع تخزين باسم "المستودع المركزي" وتحديد نوعه كـ `مستودع رئيسي (Storage)`.
  2. إضافة موقع تخزين باسم "صالة مبيعات الفرع" وتحديد نوعه كـ `صالة عرض (Showroom)`.
  3. ربطهما بالفرع المناسب والحفظ.
- **النتيجة المتوقعة**: إضافة المخزنين بنجاح والتمييز الواضح بينهما في واجهة الاستخدام والتقارير.
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [                                                                                ]

---

### TC-STK-001: تعيين الرصيد الافتتاحي وتعديل الأرصدة (Set Stock Balance)
- **الهدف**: التحقق من إمكانية ضبط رصيد أولي للصنف في المستودع الرئيسي حسب النكهة/الباركود.
- **الشروط المسبقة**: وجود صنف وباركود ومستودع رئيسي.
- **خطوات الاختبار**:
  1. التوجه إلى "إدارة المخزون والأرصدة" -> اختيار الصنف ومستودع التخزين.
  2. فتح نافذة "تحديد الرصيد (Set Stock)".
  3. إدخال كمية (100 قطعة) لنكهة الفراولة و (50 قطعة) لنكهة البرتقال.
  4. تأكيد العملية.
- **النتيجة المتوقعة**: تحديث رصيد `StorgeStock` فوراً وظهور الكميات المدخلة في جدول الأرصدة.
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [                                                                                ]

---

### TC-TRF-001: دورة التحويل المخزني (من مستودع إلى صالة عرض - مسودة ثم تأكيد ثم ترحيل)
- **الهدف**: اختبار نقل بضاعة من المستودع الرئيسي إلى صالة العرض مع تدقيق الأرصدة.
- **الشروط المسبقة**: توفر رصيد (100 قطعة) في المستودع الرئيسي.
- **خطوات الاختبار**:
  1. فتح "التحويلات المخزنية" -> "إنشاء تحويل جديد".
  2. تحديد المصدر: "المستودع المركزي"، الوجهة: "صالة مبيعات الفرع".
  3. إضافة البند (عصير نكهة الفراولة) بكمية 30 قطعة.
  4. حفظ كمسودة (Draft) -> التأكد من أن رصيد المخزن لم يتأثر بعد.
  5. تغيير الحالة إلى "مكتمل / ترحيل (Completed)".
- **النتيجة المتوقعة**:
  - في المستودع المركزي: ينخفض الرصيد من 100 إلى 70 قطعة.
  - في صالة المبيعات: يزداد رصيد الصنف `ShowroomStock` بمقدار 30 قطعة.
  - قفل أمر التحويل وعدم إمكانية تعديله بعد الإكمال.
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [                                                                                ]

---

### TC-TRF-002: منع التحويل المخزني في حال عدم كفاية الرصيد (Stock Limit Guard)
- **الهدف**: التأكد من حماية النظام للأرصدة ومنع التحويل برصيد سالب.
- **الشروط المسبقة**: الصنف متوفر منه 70 قطعة فقط في المستودع المصدر.
- **خطوات الاختبار**:
  1. إنشاء أمر تحويل مخزني وطلب كمية (150 قطعة).
  2. محاولة ترحيل وإكمال أمر التحويل (Complete).
- **النتيجة المتوقعة**: يرفض النظام ترحيل التحويل وتظهر رسالة واضحة: "الكمية المطلوبة غير متوفرة في المخزن المصدر".
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [                                                                                ]

---

### TC-ADJ-001: التسويات الجردية (Stock Adjustment: زيادة أو عجز)
- **الهدف**: التحقق من تسجيل تسوية جردية مع تحديد سبب التسوية.
- **الشروط المسبقة**: اختيار مخزن وصنف مسجل.
- **خطوات الاختبار**:
  1. فتح "التسويات الجردية" -> "إضافة تسوية جديدة".
  2. اختيار الصنف، واختيار سبب التسوية: `تالف (Damaged)` أو `جرد دوري (InventoryCount)`.
  3. إدخال تعديل كمية (-5 قطع) كتالف، أو (+10 قطع) كزيادة جرد.
  4. حفظ التسوية.
- **النتيجة المتوقعة**: تعديل رصيد المخزون الفعلي بالقيمة المحددة، وتسجيل حركة التعديل في السجل التاريخي للأرصدة.
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [نجاح لكن لا يوجد خيار لاضافة سبب لكل صنف على حدى]

---

# القسم الخامس: دورة المشتريات والموردين (Purchases & Suppliers)

### TC-SUP-001: إضافة مورد جديد وضبط الرصيد الافتتاحي
- **الهدف**: تسجيل مورد ومتابعة حسابه المالي.
- **الشروط المسبقة**: شاشة "الموردين".
- **خطوات الاختبار**:
  1. الضغط على "إضافة مورد جديد".
  2. إدخال: اسم المورد (شركة البركة للأغذية)، رقم الهاتف، الرقم الضريبي، وتحديد رصيد افتتاحي (مثلاً 5000 ريال دائن).
  3. حفظ المورد.
- **النتيجة المتوقعة**: يتم إنشاء سجل المورد ويظهر رصيده المالي الحالي بدقة في كشف الحساب.
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [                                                                                ]

---

### TC-PO-001: إنشاء ومتابعة طلبية شراء (Purchase Order Flow)
- **الهدف**: إنشاء طلبية شراء للمورد وتغيير حالتها حتى الاستلام.
- **الشروط المسبقة**: وجود مورد وأصناف معرفة.
- **خطوات الاختبار**:
  1. الذهاب إلى "طلبيات الشراء" -> "إنشاء طلبية جديدة".
  2. اختيار المورد وتحديد الأصناف والكميات المطلوبة والأسعار المتفق عليها.
  3. حفظ الطلبية ومتابعة انتقال حالتها من `Draft` إلى `Confirmed` ثم `Received`.
- **النتيجة المتوقعة**: تحديث حالة الطلبية دون زيادة رصيد المخزون الفعلي إلا بعد إنشاء فاتورة الشراء المرتبطة.
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [نجاح انشاء الطلبية لكن عملية استقبالها خاطئة تماما اولا تستقبلها بدون ربطها بمورد معين و بدون انشاء فاتروة مشترات و هدا خطا كبير تانيا في كل مرة تظغط استلام يتم تغيير حالة الطلبية و في كل مرة تتغير حالة الطلبة و تصل الي حالة مستلمة يتم زيادة المخزون بشكل خاطئ وكررني ]

---

### TC-PINV-001: إنشاء فاتورة مشتريات وتفكيك العبوات (Invoice & Stock Breakdown)
- **الهدف**: التأكد من أن فاتورة الشراء تزيد المخزون وتحدّث التكلفة وتوزع كميات النكهات بدقة.
- **الشروط المسبقة**: معرفة رصيد المخزن قبل الشراء.
- **خطوات الاختبار**:
  1. الانتقال إلى "فواتير المشتريات" -> "فاتورة مشتريات جديدة".
  2. اختيار المورد، واختيار "المستودع المركزي".
  3. إدخال رقم الفاتورة الورقية الخاصة بالمورد.
  4. إضافة صنف: كمية 10 كراتين بسعر 100 ريال للكرتون، مع تفصيل وتوزيع النكهات (Breakdowns) لكل نكهة.
  5. اختيار طريقة السداد: `آجل (Credit)`.
  6. حفظ الفاتورة.
- **النتيجة المتوقعة**:
  - زيادة رصيد المخزن الفعلي بالكميات المفصلة للكرتون والنكهات فوراً.
  - زيادة رصيد المديونية لصالح المورد بقيمة إجمالي الفاتورة.
  - تحديث سعر تكلفة الصنف بالمتوسط الجديد.
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [نجاح مع ملاحظات بعد اغلاق مربع الحوار لا يتم تحديث بيانات القائمةالخاصة بالفواتير]

---

### TC-PRET-001: مرتجع مشتريات من المستودع الرئيسي (Happy Path)
- **الهدف**: إرجاع بضاعة تالفة أو فائضة للمورد وخصمها من المخزن وتخفيض مديونيته.
- **الشروط المسبقة**: وجود رصيد كافٍ من الصنف في "المستودع الرئيسي" وفاتورة شراء سابقة للمورد.
- **خطوات الاختبار**:
  1. الذهاب إلى "مرتجعات المشتريات" -> "إشعار مرتجع مشتريات جديد".
  2. اختيار المورد واختيار فاتورة الشراء الأصلية ومستودع التخزين الرئيسي.
  3. اختيار سبب الإرجاع: `بضاعة معيبة أو تالفة (Defective)`.
  4. تحديد الصنف والباركود وإدخال كمية الإرجاع (مثلاً: 5 قطع) وسعر الوحدة.
  5. تحديد طريقة الاسترداد (تخفيض الحساب الآجل أو نقدي).
  6. الضغط على حفظ وترحيل المرتجع.
- **النتيجة المتوقعة**:
  - نجاح العملية وتوليد رقم إشعار مرتجع.
  - خصم الـ 5 قطع فوراً من رصيد المستودع الرئيسي.
  - تخفيض رصيد حساب المورد بمقدار إجمالي قيمة المرتجع.
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [                                                                                ]

---

### TC-PRET-002: منع مرتجع المشتريات من "صالة العرض" (Strict Business Rule Test)
- **الهدف**: التحقق من فرض قاعدة العمل الصارمة في النظام: (مرتجعات الموردين تتم حصرياً من المستودعات الرئيسية لضمان دقة النكهات).
- **الشروط المسبقة**: محاولة عمل مرتجع مشتريات وتحديد موقع من نوع `صالة عرض (Showroom)`.
- **خطوات الاختبار**:
  1. في نافذة مرتجع المشتريات، اختيار المورد ثم اختيار "صالة مبيعات الفرع" كجهة للمرتجع.
  2. إدخال البنود ومحاولة الحفظ.
- **النتيجة المتوقعة**: يرفض النظام الحفظ منعاً باتاً وتظهر رسالة خطأ صريحة:
  *(مرتجع المشتريات متاح فقط من المستودع الرئيسي لضمان دقة النكهات والباركودات. يرجى تحويل البضاعة من الصالة إلى المخزن أولاً عبر التحويلات المخزنية)*.
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [صالة العرض لاتظهر في القائمة المنسدلة اساسا]

---

### TC-PRET-003: منع إرجاع كمية أكبر من الرصيد المتوفر بالمخزن (Insufficient Stock on Return)
- **الهدف**: التأكد من عدم القدرة على إرجاع كمية تتجاوز رصيد الصنف المتوفر فعلياً في المستودع الرئيسي.
- **الشروط المسبقة**: رصيد الصنف في المخزن = 10 قطع فقط.
- **خطوات الاختبار**:
  1. إنشاء إشعار مرتجع مشتريات لنفس الصنف بكمية (15 قطعة).
  2. الضغط على حفظ.
- **النتيجة المتوقعة**: رفض العملية وعرض رسالة خطأ توضح أن: "رصيد المخزون المتوفر في المخزن غير كافٍ لإتمام الإرجاع".
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [                                                                                ]

---

# القسم السادس: دورة المبيعات، نقاط البيع، والعملاء (Sales, POS & Customers)

### TC-CUST-001: إدارة حسابات العملاء وأنواعهم والحد الائتماني
- **الهدف**: تسجيل عملاء بتصنيفات مختلفة (قطاعي، جملة، شركات) وتحديد سقف الائتمان.
- **الشروط المسبقة**: شاشة "العملاء والحسابات".
- **خطوات الاختبار**:
  1. إضافة عميل جديد باسم "مؤسسة النور"، نوع العميل: `شركات (Corporate)`.
  2. إدخال رقم الهاتف والحد الائتماني (10,000 ريال).
  3. حفظ العميل.
- **النتيجة المتوقعة**: إنشاء العميل وظهوره في قوائم فواتير المبيعات ونقطة البيع.
- **حالة الاختبار**: [✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [نجحت العملية لكن فشلت في عرض ارقام الهواتف بعد الادخال]

---

### TC-POS-001: نقطة البيع السريعة والبحث عبر الباركود (Quick POS Checkout)
- **الهدف**: اختبار سرعة وسلاسة عملية البيع النقدي عبر شاشة الكاشير السريعة.
- **الشروط المسبقة**: توفر رصيد في صالة العرض.
- **خطوات الاختبار**:
  1. فتح شاشة "نقطة البيع السريعة (POS)".
  2. مسح الباركود الخاص بنكهة عصير (أو إدخاله في حقل البحث السريع والضغط على Enter).
  3. التأكد من نزول الصنف في الفاتورة بالكمية (1) والسعر الصحيح.
  4. الضغط على زر الدفع السريع "نقدي (Cash)" أو اختصار لوحة المفاتيح.
  5. إتمام الفاتورة.
- **النتيجة المتوقعة**: إتمام عملية البيع فورياً، خصم الكمية من صالة العرض، تفريغ الشاشة استعداداً للعملية التالية، وتجهيز أمر الطباعة.
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [                                                                                ]

---

### TC-POS-002: استخدام لوحة الأرقام السريعة (Quantity Numpad Dialog)
- **الهدف**: تعديل كمية الصنف في نقطة البيع باستخدام لوحة الأرقام المنبثقة والشاشات اللمسية.
- **الشروط المسبقة**: وجود صنف في شبكة أصناف فاتورة POS الحالية.
- **خطوات الاختبار**:
  1. الضغط على حقل الكمية للصنف لفتح نافذة `QuantityNumpadDialog`.
  2. إدخال الكمية الجديدة (مثلاً: 12) باستخدام أزرار اللوحة الرقمية.
  3. الضغط على "تأكيد".
- **النتيجة المتوقعة**: تحديث كمية البند فوراً إلى 12 وإعادة احتساب إجمالي السطر وإجمالي الفاتورة والضرائب بدقة.
- **حالة الاختبار**: [✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [لوحة الأرقام حاليا تظهر على شكل دايلوق و انا لا اريد هدا اريدها ان تظهر على شكل نافدة عائمة تحت الكمية او مربع السعر الدي يتم الظغط عليه]

---

### TC-POS-003: إضافة عميل سريع من داخل شاشة الكاشير (Quick Customer Dialog)
- **الهدف**: تسجيل عميل جديد لحظياً دون الحاجة للخروج من نقطة البيع.
- **الشروط المسبقة**: شاشة POS مفتوحة أثناء عملية بيع.
- **خطوات الاختبار**:
  1. الضغط على زر إضافة عميل بجانب قائمة العملاء لفتح `QuickCustomerDialog`.
  2. إدخال اسم العميل ورقم هاتفه، والضغط على حفظ.
- **النتيجة المتوقعة**: حفظ العميل واختياره تلقائياً كعميل للفاتورة المفتوحة حالياً دون فقدان الأصناف الممسوحة.
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [                                                                                ]

---

### TC-SINV-001: إنشاء فاتورة مبيعات عامة بطرق دفع متعددة (آجل / نقدي / شبكة)
- **الهدف**: فحص الفواتير الآجلة وتأثيرها على حساب العميل والمخزون.
- **الشروط المسبقة**: اختيار عميل له حساب آجل.
- **خطوات الاختبار**:
  1. التوجه إلى "فواتير المبيعات" -> "فاتورة مبيعات جديدة".
  2. اختيار العميل والمخزن (صالة العرض).
  3. إضافة الأصناف مع تطبيق خصم (مثلاً 5%).
  4. تحديد طريقة السداد: `آجل (Credit)` مع سداد دفعة مقدمة جزئية.
  5. حفظ الفاتورة.
- **النتيجة المتوقعة**:
  - خصم الكميات من رصيد صالة العرض.
  - ترحيل المبلغ المتبقي كمديونية على حساب العميل.
  - حساب ضريبة القيمة المضافة والإجمالي الصافي بدقة رياضية لا تحتمل الخطأ.
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [يخصم وقت اصدار الاتورة فقط و منالصالة فقط بمعن انه ان تم البيع من الكخن لا يخصم و هاد طبيعي لان المخزن طريقة التعامل معه مخترفة و ايضا في حالة تعديل الفاتورة اضافة صنف او ازالة كمية او زيادة كمية لا يخصم ]

---

### TC-SINV-002: منع إتمام البيع في حال نفاد الرصيد (Out of Stock Validation)
- **الهدف**: التأكد من أن النظام يمنع البيع بالسالب في الصالة.
- **الشروط المسبقة**: صنف رصيده في صالة العرض = 2 قطعة.
- **خطوات الاختبار**:
  1. إضافة الصنف في فاتورة المبيعات وطلب كمية (10 قطع).
  2. محاولة حفظ وإصدار الفاتورة.
- **النتيجة المتوقعة**: منع الحفظ وظهور رسالة خطأ: "الكمية المطلوبة غير متوفرة في صالة العرض للصنف المحدد".
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [في نقطة البيع تعمل في الأصدار اليدوي لا تعمل]

---

### TC-SRET-001: مرتجع مبيعات بناءً على فاتورة بيع سابقة
- **الهدف**: التحقق من إعادة العميل لبضاعة واسترداد قيمتها وإعادتها للمخزون.
- **الشروط المسبقة**: وجود فاتورة بيع مكتملة سابقة.
- **خطوات الاختبار**:
  1. فتح "مرتجعات المبيعات" -> "إضافة مرتجع مبيعات".
  2. تحديد الفاتورة الأصلية واختيار الصنف المرتجع وسبب الإرجاع (مثلاً: `صنف خاطئ`).
  3. تحديد الكمية المسترجعة واختيار رد المبلغ نقداً أو إضافة رصيد لحساب العميل.
  4. حفظ المرتجع.
- **النتيجة المتوقعة**:
  - إضافة الكمية المرتجعة فوراً إلى رصيد المخزن/الصالة.
  - خصم القيمة من إجمالي المبيعات وتعديل رصيد العميل أو الصندوق.
  - منع إرجاع كمية أكبر من الكمية التي تم شراؤها في الفاتورة الأصلية.
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [لكن في حالة الغاء عملية الارجاع لا يتم التراجع عن جميع العمليات الناتجةعن اضافة مرتجع]

---

# القسم السابع: لوحة المؤشرات وتجربة المستخدم (Dashboard & UI/UX)

### TC-DSH-001: دقة مؤشرات لوحة التحكم والتحديث اللحظي
- **الهدف**: التحقق من تحديث أرقام المبيعات اليومية والشهرية وإحصائيات المخزون.
- **الشروط المسبقة**: إجراء عمليات بيع وشراء مسجلة في نفس اليوم.
- **خطوات الاختبار**:
  1. الانتقال إلى شاشة "لوحة التحكم (Dashboard)".
  2. مقارنة إجمالي المبيعات المعروضة مع مجموع فواتير المبيعات لليوم.
  3. مراجعة قسم "تنبيهات الأصناف منخفضة المخزون" والأصناف الأكثر مبيعاً.
- **النتيجة المتوقعة**: تطابق إحصائيات لوحة التحكم بنسبة 100% مع البيانات الفعلية في قاعدة البيانات.
- **حالة الاختبار**: [ ] ناجح    [ ✓] فاشل
- **ملاحظات الفاحص**: [الشيئ الوحيد الدي يعمل هو اجمالي المشتريات]

---

### TC-UI-001: تجربة الوضع الداكن والفاتح (Dark / Light Theme)
- **الهدف**: التأكد من سلامة التباين اللوني وسهولة القراءة عند التبديل بين الثيمات.
- **الشروط المسبقة**: فتح الشاشة الرئيسية.
- **خطوات الاختبار**:
  1. الضغط على زر تبديل الثيم (أيقونة القمر/الشمس) في أعلى الزاوية اليسرى.
  2. فحص النصوص والجداول والحقول في الوضع الداكن ثم الفاتح.
- **النتيجة المتوقعة**: تبديل سلس وجميل للألوان دون أي تداخل في النصوص أو اختفاء للبيانات، مع حفظ تفضيل المستخدم.
- **حالة الاختبار**: [ ✓] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [                                                                                ]

---

### TC-UI-002: طي وتوسيع القائمة الجانبية (Sidebar Toggle & F11)
- **الهدف**: اختبار استجابة الشاشة عند إخفاء القائمة لزيادة مساحة العمل (خاصة لشاشات الكاشير الصغيرة).
- **الشروط المسبقة**: فتح التطبيق بأي دقة شاشة.
- **خطوات الاختبار**:
  1. الضغط على زر القائمة `☰` أو مفتاح `F11`.
  2. التحقق من اختفاء القائمة وتمدد مساحة العرض الرئيسية تلقائياً.
  3. الضغط مرة أخرى لإعادة القائمة.
- **النتيجة المتوقعة**: حركة انسيابية سريعة للطي والتوسيع دون أي تشوه في محاذاة الجداول أو الأزرار.
- **حالة الاختبار**: [✓ ] ناجح    [ ] فاشل
- **ملاحظات الفاحص**: [تعمل لكنا تطوى كلها انا اريد ان تبقى الأيقونات موجودة عندما تطوي]

---

# 📊 ملخص نتائج الاختبار النهائي (Test Execution Summary)

| رقم القسم | اسم الوحدة المفحوصة | إجمالي الاختبارات | عدد الناجح | عدد الفاشل | نسبة النجاح (%) |
| :--- | :--- | :---: | :---: | :---: | :---: |
| 1 | الأمان والمصادقة والمستخدمين | 5 | [ ] | [ ] | [ ] % |
| 2 | إعدادات المؤسسة والفروع | 2 | [ ] | [ ] | [ ] % |
| 3 | دليل المنتجات والكتالوج والوحدات | 5 | [ ] | [ ] | [ ] % |
| 4 | إدارة المخازن والأرصدة والتحويلات | 5 | [ ] | [ ] | [ ] % |
| 5 | المشتريات والموردين ومرتجع المشتريات | 6 | [ ] | [ ] | [ ] % |
| 6 | المبيعات ونقاط البيع والعملاء | 6 | [ ] | [ ] | [ ] % |
| 7 | لوحة التحكم وتجربة المستخدم | 3 | [ ] | [ ] | [ ] % |
| **المجموع** | **جميع وظائف النظام** | **32** | [ ] | [ ] | [ ] % |

---

### 📝 الملاحظات والتوصيات العامة للفاحص:
[الملاحظات العامة:                                                                                                                                                                                                                                                                                                                                                                                                                                ]

### ✍️ توقيع واعتماد الفاحص:
- **توقيع المختبر**: [التوقيع: ___________________________ ]
- **التاريخ**: [التاريخ: ___________________________ ]
