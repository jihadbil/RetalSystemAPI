# 📋 وثيقة اختبارات التحقق الشاملة (Post-Fix Verification Test Suite)
## نظام إدارة المبيعات والمخازن — Retal System Pro

---

> [!TIP]
> **إرشادات الاستخدام:**
> صُممت هذه الوثيقة لمساعدتك في اختبار وتدقيق التعديلات الـ 14 المنفذة في النظام بعد تطبيق خطة الإصلاحات الشاملة. يمكنك وضع علامة `[x]` أمام النتيجة المناسبة وتدوين ملاحظاتك بين الأقواس `[]` أثناء التشغيل العملي.

---

## 📊 جدول المتابعة السريعة لحالات الاختبار

| رقم الفحص | الوحدة / الميزة | الهدف الأساسي | الحالة |
| :---: | :--- | :--- | :---: |
| **TC-FIX-001** | طلبيات الشراء | ربط المورد ومنع تكرار تضخيم المخزون عند الاستلام | `[ ]` |
| **TC-FIX-002** | تصنيفات الكتالوج | حفظ التعديلات دون خطأ تصادم التتبع في EF Core | `[ ]` |
| **TC-FIX-003** | إدارة المنتجات | فحص الباركود المكرر استباقياً برسالة 400 واضحة | `[ ]` |
| **TC-FIX-004** | فواتير المبيعات | احتساب الفارق التراكمي للمخزون بدقة عند التعديل/الحذف | `[ ]` |
| **TC-FIX-005** | مرتجعات المبيعات | عكس المخزون وخصم الكميات تلقائياً عند حذف المرتجع | `[ ]` |
| **TC-FIX-006** | لوحة التحكم | ظهور المبيعات النقدية بدقة وإدراج مستودعات التخزين بالنواقص | `[ ]` |
| **TC-FIX-007** | إدارة المستخدمين | زر إعادة تعيين كلمة المرور المباشر من الجدول والنافذة | `[ ]` |
| **TC-FIX-008** | الصلاحيات والأمان | توجيه الكاشير تلقائياً للـ POS وحجب شاشات الإدارة | `[ ]` |
| **TC-FIX-009** | العملاء والفروع | عرض أرقام الهواتف الأساسية في الجداول دون فراغ | `[ ]` |
| **TC-FIX-010** | فواتير الشراء والدخول | تحديث جدول المشتريات تلقائياً وثبات تنبيه خطأ الدخول | `[ ]` |
| **TC-FIX-011** | نقطة البيع (POS) | لوحة الأرقام العائمة (Popup) وتعديل الكمية السريع | `[ ]` |
| **TC-FIX-012** | القائمة الجانبية | طي القائمة المصغرة (64px) مع بقاء الأيقونات والتلميحات | `[ ]` |
| **TC-FIX-013** | التسويات الجردية | اختيار وعرض وتوثيق سبب التسوية لكل بند منفصل | `[ ]` |
| **TC-FIX-014** | كتالوج المنتجات | مزامنة الباركود لتبويب الصور بالذاكرة قبل الحفظ | `[ ]` |

---

## 🏷️ المرحلة الأولى: الأخطاء الحرجة وتكامل البيانات والمخزون

### 🔹 الاختبار 1: طلبيات الشراء وربط المورد ومنع تكرار زيادة المخزون (TC-FIX-001)
* **المسار في التطبيق**: القائمة الرئيسية ⬅️ المشتريات ⬅️ طلبيات الشراء
* **الهدف**: التحقق من اختيار المورد وتخزينه، ومنع تكرار مضاعفة المخزون عند النقر المتكرر على الاستلام.
* **خطوات الاختبار**:
  1. اضغط على زر **إضافة طلبية جديدة**.
  2. افتح القائمة المنسدلة للموردين واختر مورداً محدداً، ثم أضف صنفاً بكمية محددة (مثلاً: `10` قطع).
  3. احفظ الطلبية؛ تأكد من ظهور اسم المورد في عمود المورد بالجدول الرئيسي.
  4. انقر على زر **استلام الطلبية**؛ لاحظ انتقال الحالة إلى "مستلمة" (`Received`).
  5. افتح شاشة الأصناف/المخزن وتأكد من زيادة رصيد الصنف بمقدار `10` قطع.
  6. ارجع لطلبية الشراء وانقر عليها مجدداً؛ تأكد أن الحالة ثابتة ولم يتم تدويرها، ولم يزد المخزون مرة ثانية.
* **النتيجة المتوقعة**: المورد مرتبط بشكل سليم، والكمية تضاف للمخزن لمرة واحدة فقط وبشكل صارم (Idempotent).
* **تقييم الفحص**:
  - [✓] ناجح (Pass)
  - [ ] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[نجاح لكن الربط بالمورد اصبح اجباري و هناك بعض الفواتير تكون عامة لدلك يجب منع الاستلام في حالة عدم رطها بالمورد و هكدا يصبح الربط غير اجباري تانيا لا يتم اضافة فاتورة مشتريات و بنودها]`

---

### 🔹 الاختبار 2: تعديل تصنيفات الكتالوج بدون استثناء EF Core (TC-FIX-002)
* **المسار في التطبيق**: القائمة الرئيسية ⬅️ الكتالوج ⬅️ التصنيفات
* **الهدف**: معالجة خطأ `InvalidOperationException: The instance of entity type Category cannot be tracked...`.
* **خطوات الاختبار**:
  1. اختر أي تصنيف مسجل مسبقاً في الجدول.
  2. انقر على زر **تعديل**.
  3. قم بتعديل اسم التصنيف، أو تغيير ترتيب العرض، أو تبديل حالة التنشيط.
  4. اضغط على زر **حفظ**.
* **النتيجة المتوقعة**: يتم حفظ التعديل بنجاح فوراً وتحديث الجدول دون ظهور أي رسالة خطأ داخلية.
* **تقييم الفحص**:
  - [✓] ناجح (Pass)
  - [ ] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[                                                                                                    ]`

---

### 🔹 الاختبار 3: فحص تكرار الباركود استباقياً برسالة واضحة (TC-FIX-003)
* **المسار في التطبيق**: القائمة الرئيسية ⬅️ الكتالوج ⬅️ المنتجات ⬅️ إضافة منتج
* **الهدف**: التحقق من منع تكرار الباركود عبر استجابة `400 Bad Request` بدلاً من انهيار السيرفر بـ `500 SqlException`.
* **خطوات الاختبار**:
  1. افتح نافذة إضافة منتج جديد.
  2. املأ البيانات الأساسية، وفي تبويب الباركودات أدخل باركوداً مستخدماً بالفعل لمنتج مسجل سابقاً.
  3. اضغط على زر **حفظ**.
* **النتيجة المتوقعة**: يرفض النظام الحفظ بأمان تام، وتظهر رسالة تنبيه عربية واضحة تفيد بأن الباركود مستخدم مسبقاً لصنف آخر.
* **تقييم الفحص**:
  - [✓] ناجح (Pass)
  - [ ] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[                                                                                                    ]`

---

### 🔹 الاختبار 4: احتساب الفارق التراكمي للمخزون عند تعديل/حذف فواتير المبيعات (TC-FIX-004)
* **المسار في التطبيق**: القائمة الرئيسية ⬅️ المبيعات ⬅️ فواتير المبيعات
* **الهدف**: التحقق من منطق الـ Delta Stock؛ بحيث تعكس التعديلات فوارق الكميات فقط على رصيد المخزن الفعلي.
* **خطوات الاختبار**:
  1. سجّل الرصيد الابتدائي لأحد الأصناف (مثلاً: `50` قطعة).
  2. أنشئ فاتورة مبيعات جديدة واشترِ `10` قطع (الرصيد في المخزن يصبح: `40` قطعة).
  3. افتح الفاتورة السابقة واضغط **تعديل**؛ ارفع الكمية إلى `15` قطعة واحفظ (يجب أن يصبح الرصيد: `35` قطعة).
  4. افتح الفاتورة مجدداً وخفّض الكمية إلى `5` قطع واحفظ (يجب أن يعود الرصيد: `45` قطعة).
  5. اضغط على خيار **حذف الفاتورة** نهائياً.
* **النتيجة المتوقعة**: يتم خصم وإعادة الكميات بحسب الفارق بالضبط في كل تعديل، وعند حذف الفاتورة يُسترجع كامل الرصيد (`50` قطعة).
* **تقييم الفحص**:
  - [ ] ناجح (Pass)
  - [✓ ] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[[حاولت اضافة بند لفاتورة مبيعات حصلت على الخطا التالي fail: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      System.InvalidOperationException: The instance of entity type 'Warehouse' cannot be tracked because another instance with the same key value for {'Id'} is already being tracked. When attaching existing entities, ensure that only one entity instance with a given key value is attached. Consider using 'DbContextOptionsBuilder.EnableSensitiveDataLogging' to see the conflicting key values.
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
         at RetalSystemAPI.Services.Sales.Implementations.SalesInvoiceService.UpdateAsync(Guid id, UpdateSalesInvoiceDto dto, CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI.Services\Sales\Implementations\SalesInvoiceService.cs:line 416
         at RetalSystemAPI.Controllers.Sales.SalesInvoicesController.Update(Guid id, UpdateSalesInvoiceDto dto, CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI\Controllers\Sales\SalesInvoicesController.cs:line 109
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
ثم حاولت حدف الفاتورة حصلت على التالي  Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      System.InvalidOperationException: The instance of entity type 'Warehouse' cannot be tracked because another instance with the same key value for {'Id'} is already being tracked. When attaching existing entities, ensure that only one entity instance with a given key value is attached. Consider using 'DbContextOptionsBuilder.EnableSensitiveDataLogging' to see the conflicting key values.
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
         at RetalSystemAPI.DataAccess.Repositories.Implementations.Repository`1.SoftDelete(T entity) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI.DataAccess\Repositories\Implementations\Repository.cs:line 272
         at RetalSystemAPI.Services.Sales.Implementations.SalesInvoiceService.DeleteAsync(Guid id, CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI.Services\Sales\Implementations\SalesInvoiceService.cs:line 521
         at RetalSystemAPI.Controllers.Sales.SalesInvoicesController.Delete(Guid id, CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI\Controllers\Sales\SalesInvoicesController.cs:line 129
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

]]`

---

### 🔹 الاختبار 5: عكس المخزون تلقائياً عند حذف مرتجع المبيعات (TC-FIX-005)
* **المسار في التطبيق**: القائمة الرئيسية ⬅️ المبيعات ⬅️ مرتجعات المبيعات
* **الهدف**: التأكد من خصم الكميات المرتجعة من المخزن عند إلغاء أو حذف سجل المرتجع.
* **خطوات الاختبار**:
  1. أنشئ مرتجع مبيعات لصنف معين بكمية `5` قطع، وتأكد من زيادة رصيد المخزن بتلك الكمية.
  2. حدد سجل المرتجع في جدول المرتجعات واضغط على **حذف المرتجع**.
  3. افتح رصيد المخزن وتأكد من الرصيد الحالي للصنف.
* **النتيجة المتوقعة**: يتم خصم الـ `5` قطع تلقائياً من المخزن لإلغاء أثر المرتجع المحذوف.
* **تقييم الفحص**:
  - [ ] ناجح (Pass)
  - [✓] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[عند حدف مرتجع مبيعات il: Microsoft.AspNetCore.Diagnostics.DeveloperExceptionPageMiddleware[1]
      An unhandled exception has occurred while executing the request.
      System.InvalidOperationException: The instance of entity type 'Warehouse' cannot be tracked because another instance with the same key value for {'Id'} is already being tracked. When attaching existing entities, ensure that only one entity instance with a given key value is attached. Consider using 'DbContextOptionsBuilder.EnableSensitiveDataLogging' to see the conflicting key values.
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IdentityMap`1.ThrowIdentityConflict(InternalEntityEntry entry)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IdentityMap`1.Add(TKey key, InternalEntityEntry entry, Boolean updateDuplicate)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IdentityMap`1.Add(TKey key, InternalEntityEntry entry)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.IdentityMap`1.Add(InternalEntityEntry entry)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.StateManager.StartTracking(InternalEntityEntry entry)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.InternalEntryBase.SetEntityState(EntityState oldState, EntityState newState, Boolean acceptChanges, Boolean modifyProperties)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.InternalEntityEntry.SetEntityState(EntityState entityState, Boolean acceptChanges, Boolean modifyProperties, Nullable`1 forceStateWhenUnknownKey, Nullable`1 fallbackState)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityGraphAttacher.PaintAction(EntityEntryGraphNode`1 node)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityEntryGraphIterator.TraverseGraph[TState](EntityEntryGraphNode`1 node, Func`2 handleNode)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityEntryGraphIterator.TraverseGraph[TState](EntityEntryGraphNode`1 node, Func`2 handleNode)
         at Microsoft.EntityFrameworkCore.ChangeTracking.Internal.EntityGraphAttacher.AttachGraph(InternalEntityEntry rootEntry, EntityState targetState, EntityState storeGeneratedWithKeySetTargetState, Boolean forceStateWhenUnknownKey)
         at Microsoft.EntityFrameworkCore.Internal.InternalDbSet`1.SetEntityState(InternalEntityEntry entry, EntityState entityState)
         at Microsoft.EntityFrameworkCore.Internal.InternalDbSet`1.Attach(TEntity entity)
         at RetalSystemAPI.DataAccess.Repositories.Implementations.Repository`1.SoftDelete(T entity) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI.DataAccess\Repositories\Implementations\Repository.cs:line 272
         at RetalSystemAPI.Services.Sales.Implementations.SalesReturnService.DeleteAsync(Guid id, CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI.Services\Sales\Implementations\SalesReturnService.cs:line 273
         at RetalSystemAPI.Controllers.Sales.SalesReturnsController.Delete(Guid id, CancellationToken ct) in C:\Users\Masoud\source\repos\RetalSystemAPI\RetalSystemAPI\Controllers\Sales\SalesReturnsController.cs:line 107
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

]`

---

## 🏷️ المرحلة الثانية: لوحة التحكم والصلاحيات والأمان

### 🔹 الاختبار 6: انعكاس مبيعات الكاشير النقدية ورادار النواقص في لوحة التحكم (TC-FIX-006)
* **المسار في التطبيق**: نقطة البيع ⬅️ ثم لوحة التحكم (Dashboard)
* **الهدف**: التأكد من تطابق ترقيم طرق الدفع (`PaymentMethod`) وتضمين مستودعات التخزين في النواقص.
* **خطوات الاختبار**:
  1. افتح شاشة **نقطة البيع (POS)** وأجرِ عملية بيع نقدية (دفع نقدي بالكامل) بقيمة محددة (مثلاً: `300` ر.س).
  2. افتح **لوحة التحكم (Dashboard)**.
  3. راجع بطاقة "مبيعات اليوم النقدية"، وبطاقة "إجمالي مبيعات اليوم"، والرسوم البيانية.
  4. افحص جدول الأصناف منخفضة المخزون (رادار النواقص).
* **النتيجة المتوقعة**: تظهر قيمة الـ `300` ر.س فوراً ضمن مبيعات اليوم النقدية دون أن تظهر بقيمة صفرية، وتظهر نواقص مستودعات التخزين بجانب الصالات.
* **تقييم الفحص**:
  - [✓ ] ناجح (Pass)
  - [ ] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[                                                                                                    ]`

---

### 🔹 الاختبار 7: زر إعادة تعيين كلمة المرور من واجهة إدارة المستخدمين (TC-FIX-007)
* **المسار في التطبيق**: القائمة الرئيسية ⬅️ الإدارة ⬅️ المستخدمين
* **الهدف**: التحقق من توفر الزر وربطه البرمجي بنافذة `ResetPasswordWindow`.
* **خطوات الاختبار**:
  1. سجّل الدخول بحساب المدير (Admin).
  2. افتح جدول المستخدمين؛ تأكد من وجود زر **🔑 إعادة تعيين كلمة المرور** في الجدول لكل مستخدم.
  3. افتح نافذة تعديل بيانات المستخدم؛ تأكد من وجود نفس الزر داخل النافذة أيضاً.
  4. انقر على الزر، وأدخل كلمة مرور جديدة ثم اضغط "حفظ".
  5. سجّل الخروج وحاول تسجيل الدخول بذلك الحساب باستخدام كلمة المرور الجديدة.
* **النتيجة المتوقعة**: تفتح نافذة إعادة التعيين مباشرة، ويتم تغيير كلمة المرور وتسجيل الدخول بها بنجاح.
* **تقييم الفحص**:
  - [✓] ناجح (Pass)
  - [ ] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[                                                                                                    ]`

---

### 🔹 الاختبار 8: تقييد صلاحيات الكاشير والتوجيه التلقائي لنقطة البيع (TC-FIX-008)
* **المسار في التطبيق**: شاشة تسجيل الدخول
* **الهدف**: حماية شاشات الإدارة الحساسة من الوصول غير المصرح به للكاشير.
* **خطوات الاختبار**:
  1. سجّل الدخول بحساب يحمل دور **كاشير (Cashier)** فقط.
  2. راقب الشاشة التي يفتح عليها التطبيق تلقائياً بمجرد الدخول.
  3. راقب القائمة الجانبية للشاشات المتاحة.
* **النتيجة المتوقعة**: يتم توجيه الكاشير تلقائياً ومباشرة إلى شاشة نقطة البيع (POS)، وتختفي شاشات الإدارة (المستخدمين، المؤسسة، الفروع، لوحة التحكم الرئيسية) من القائمة الجانبية.
* **تقييم الفحص**:
  - [✓] ناجح (Pass)
  - [ ] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[ليس بالشكل المطلوب لكن لا باس به حتى الأن]`

---

### 🔹 الاختبار 9: ظهور أرقام هواتف العملاء والفروع في الجداول (TC-FIX-009)
* **المسار في التطبيق**: العملاء / الفروع
* **الهدف**: التحقق من دقة الـ AutoMapper وعرض الهواتف في الواجهة.
* **خطوات الاختبار**:
  1. افتح شاشة **العملاء**؛ راجع عمود الهاتف الأساسي وتأكد من ظهور الأرقام المدخلة سابقاً.
  2. افتح شاشة **الفروع**؛ أضف فرعاً جديداً برقم هاتف أو عدّل فرعاً قائماً، وراقب عمود الهاتف بالجدول.
* **النتيجة المتوقعة**: تظهر أرقام الهواتف واضحة ومنسقة في الجداول لكلا الشاشتين.
* **تقييم الفحص**:
  - [ ] ناجح (Pass)
  - [✓] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[اضافة ارقام الهاتف في العملاء كانت تعمل الدي كان لا يعمل هو اظهارها عند عرض التفاصيل اما الأن لا الأضافة تعمل ولا العرض يعمل قم باعادة الكود الخاص بالعملاء كما كان و اصلح عملية العرص ملاحظة انا هنا اتحدث عن العملاء و ليس الموردين الكود الموجد حاليا يقوم بحدف كل البياانتا من جدول هواتق العملاء]`

---

### 🔹 الاختبار 10: التحديث التلقائي لفواتير الشراء وثبات تنبيه تسجيل الدخول (TC-FIX-010)
* **المسار في التطبيق**: شاشة تسجيل الدخول / فواتير المشتريات
* **الهدف**: منع وميض رسالة الخطأ في شاشة الدخول، والتحديث الآلي لقائمة المشتريات.
* **خطوات الاختبار**:
  1. في شاشة الدخول: أدخل بيانات دخول غير صحيحة واضغط دخول؛ راقب رسالة التنبيه الحمراء.
  2. سجّل الدخول وافتح **فواتير المشتريات**.
  3. اضغط "إضافة فاتورة جديدة"، وأكمل الحفظ ثم أغلق النافذة المنبثقة.
* **النتيجة المتوقعة**: رسالة الخطأ ثابتة بدون أي وميض أو اختفاء، وجدول فواتير المشتريات يتحدث تلقائياً وتظهر الفاتورة فور الإغلاق دون الحاجة لضغط زر التحديث يدوياً.
* **تقييم الفحص**:
  - [✓] ناجح (Pass)
  - [ ] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[                                                                                                    ]`

---

## 🏷️ المرحلة الثالثة: تحسينات وتطويرات تجربة المستخدم (UI/UX)

### 🔹 الاختبار 11: لوحة الأرقام العائمة في نقطة البيع (Floating Numpad Popup) (TC-FIX-011)
* **المسار في التطبيق**: القائمة الرئيسية ⬅️ المبيعات ⬅️ نقطة البيع (POS)
* **الهدف**: تجربة تعديل كمية الصنف عبر نافذة عائمة أسفل الزر سريعة وتدعم اللمس والكيبورد.
* **خطوات الاختبار**:
  1. أضف صنفاً إلى سلة المبيعات.
  2. انقر على زر رقم الكمية الخاص بالصنف داخل السلة.
  3. تأكد من فتح نافذة أرقام عائمة (Popup) صغيرة أسفل زر الكمية مباشرة دون حجب الشاشة.
  4. جرب زيادة الكمية أو النقر على الأرقام باللمس/الفأرة، أو كتابة الرقم والضغط على `Enter`.
  5. جرب تصفير الكمية إلى `0` أو مسحها بالكامل ثم الإدخال؛ لاحظ حذف الصنف من السلة.
  6. انقر في أي مكان خارج الـ Popup للتأكد من إغلاقه تلقائياً وسلاسة الاستمرار في البيع.
* **النتيجة المتوقعة**: تجربة مستخدم سريعة جداً بدون مقاطعة سير العمل في الكاشير.
* **تقييم الفحص**:
  - [✓] ناجح (Pass)
  - [ ] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[                                                                                                    ]`

---

### 🔹 الاختبار 12: القائمة الجانبية المصغرة (Mini-Sidebar) (TC-FIX-012)
* **المسار في التطبيق**: الشريط العلوي للتطبيق (زر ☰ أو مفتاح F11)
* **الهدف**: التحقق من نمط القائمة الجانبية المصغرة لشاشات الكاشير والشاشات الصغيرة.
* **خطوات الاختبار**:
  1. اضغط على زر الهامبرغر `☰` في الشريط العلوي (أو اضغط `F11` من لوحة المفاتيح).
  2. لاحظ تقلص القائمة إلى عرض `64px` مع بقاء كافة الأيقونات في المنتصف وإخفاء النصوص.
  3. مرر مؤشر الفأرة فوق أيقونات التنقل وتأكد من ظهور التلميحات (Tooltips) بأسماء الشاشات.
  4. انقر على أيقونة مصغرة وتأكد من فتح الشاشة المطلوبة بشكل طبيعي.
  5. انقر مجدداً على زر التوسيع؛ تأكد من عودة القائمة لعرضها الكامل (`232px`).
* **النتيجة المتوقعة**: مظهر جمالي مريح يوفر مساحة عمل واسعة للـ POS دون التأثير على إمكانية الوصول لأي قسم.
* **تقييم الفحص**:
  - [✓] ناجح (Pass)
  - [ ] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[                                                                                                    ]`

---

### 🔹 الاختبار 13: توثيق سبب التسوية الجردية على مستوى كل بند (TC-FIX-013)
* **المسار في التطبيق**: القائمة الرئيسية ⬅️ المخازن ⬅️ التسويات الجردية
* **الهدف**: التحقق من تسجيل وعرض وتوثيق سبب التسوية لكل بند منفصل.
* **خطوات الاختبار**:
  1. اضغط على زر **تسجيل تسوية جردية جديدة**.
  2. اختر المستودع، ثم حدد صنفاً وأدخل الفرق في الكمية.
  3. اختر سبباً مخصصاً من القائمة المنسدلة للسبب (مثلاً: "بضاعة تالفة" أو "منتهي الصلاحية").
  4. اضغط على "إضافة البند"؛ تأكد من ظهور عمود "سبب البند" في جدول البنود موضحاً السبب المختار.
  5. احفظ التسوية الجردية.
* **النتيجة المتوقعة**: يتم حفظ سبب التعديل الخاص بكل صنف بشكل منفصل وواضح في تقرير وسجل التسوية.
* **تقييم الفحص**:
  - [✓] ناجح (Pass)
  - [ ] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[يعمل لكن طريقة العرض غبية يجب ان يعرض بجانب الصنف الخاص به و ليس تحت في الأسفل ما هدا الغباء]`

---

### 🔹 الاختبار 14: مزامنة باركودات الصنف لتبويب الصور في الذاكرة قبل الحفظ (TC-FIX-014)
* **المسار في التطبيق**: القائمة الرئيسية ⬅️ الكتالوج ⬅️ المنتجات ⬅️ إضافة منتج جديد
* **الهدف**: إمكانية ربط صورة المنتج بالباركود مباشرة أثناء الإنشاء دون الحاجة لحفظ مسبق.
* **خطوات الاختبار**:
  1. افتح نافذة "إضافة منتج جديد".
  2. املأ البيانات الأساسية للمنتج، ثم في تبويب الباركودات أضف باركوداً جديداً (مثال: `TEST-BAR-01`) دون الضغط على زر حفظ المنتج.
  3. انتقل فوراً إلى تبويب "الصور".
  4. افتح القائمة المنسدلة للباركودات المرتبطة بالصورة.
  5. تأكد من ظهور الباركود `TEST-BAR-01` في القائمة المنسدلة فوراً، واختره وارفع صورة ثم اضغط حفظ.
* **النتيجة المتوقعة**: مزامنة فورية بالذاكرة تتيح ربط الصور بالباركودات قبل الحفظ بنجاح تام.
* **تقييم الفحص**:
  - [✓] ناجح (Pass)
  - [ ] فاشل (Fail)
* **ملاحظات المختبر**:
  > `[                                                                                                    ]`

---

## 🏁 ملخص التقييم النهائي بعد الاختبار

* إجمالي الفحوصات المنفذة: `[    / 14 ]`
* الفحوصات الناجحة: `[    ]`
* الفحوصات الفاشلة أو التي تحتاج لملاحظات: `[    ]`
* قرار الجاهزية للإطلاق (Go/No-Go): `[ ] جاهز للإطلاق`  |  `[ ] يتطلب تعديلات إضافية`
