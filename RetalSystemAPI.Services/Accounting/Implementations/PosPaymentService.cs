using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Repositories.Interfaces;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.Constants;
using RetalSystemAPI.Models.DTOs.Accounting.Pos;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;
using RetalSystemAPI.Services.Accounting.Interfaces;
using RetalSystemAPI.Services.Common.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة مدفوعات نقاط البيع وتوزيع حركات السداد وتسويتها وعكسها محاسبياً.
/// </summary>
/// <param name="db">سياق قاعدة البيانات للوصول المباشر لحركات الجلسات</param>
/// <param name="uow">وحدة العمل لإدارة المعاملات والمستودعات</param>
/// <param name="pos">خدمة إدارة جلسات الكاشير والصناديق</param>
/// <param name="posting">خدمة الترحيل المالي وتحديث أرصدة الخزائن</param>
/// <param name="currentUser">خدمة استخراج بيانات المستخدم المتصل حالياً</param>
/// <param name="permissions">خدمة فحص صلاحيات المستخدم</param>
public sealed class PosPaymentService(
    AppDbContext db,
    IUnitOfWork uow,
    IPosCashService pos,
    IFinancialPostingService posting,
    ICurrentUserService currentUser,
    ICurrentPermissionService permissions) : IPosPaymentService
{
    /// <inheritdoc />
    /// <summary>
    /// التحقق من وجود حركات دفع مسجلة لفاتورة مبيعات محددة.
    /// </summary>
    public Task<bool> HasPaymentsAsync(Guid invoiceId, CancellationToken ct) =>
        // الاستعلام في جدول حركات الجلسات للتحقق من وجود أي حركة مرتبطة بالفاتورة
        db.CashSessionMovements.AnyAsync(x => x.SalesInvoiceId == invoiceId, ct);

    /// <inheritdoc />
    /// <summary>
    /// استرجاع حركات الدفع الواردة لفاتورة مبيعات محددة مع حالة التسوية.
    /// </summary>
    public Task<List<PosPaymentResponseDto>> GetInvoicePaymentsAsync(Guid invoiceId, CancellationToken ct) =>
        // استعلام قراءة فقط لحركات القبض الخاصة بالفاتورة
        db.CashSessionMovements.AsNoTracking()
            // تصفية الحركات المرتبطة بالفاتورة والمستند الصحيح والحركة الواردة
            .Where(x => x.SalesInvoiceId == invoiceId && x.DocumentType == FinancialDocumentType.SalesInvoice && x.Direction == TransactionDirection.Inbound)
            // ترتيب النتائج حسب وسيلة السداد
            .OrderBy(x => x.PaymentMethod)
            // تحويل الكيانات إلى كائنات DTO
            .Select(x => new PosPaymentResponseDto
            {
                Id = x.Id,
                PaymentMethod = x.PaymentMethod,
                Amount = x.Amount,
                TenderedAmount = x.TenderedAmount,
                IsSettled = x.SettlementId.HasValue
            })
            // تنفيذ الاستعلام وإرجاع القائمة
            .ToListAsync(ct);

    /// <inheritdoc />
    /// <summary>
    /// تسجيل حركات السداد المتعددة في جدول حركات جلسات الصندوق وتفعيل مؤشر تعديل الجلسة لمنع تعارض التزامن.
    /// </summary>
    public Task RecordAsync(SalesInvoice invoice, CashSession session, IReadOnlyList<PosPaymentDto> payments, CancellationToken ct)
    {
        // التكرار على كل وسيلة دفع مسددة من قبل العميل
        foreach (var payment in payments)
        {
            // إنشاء وإضافة سجل حركة جلسة صندوق جديدة
            db.CashSessionMovements.Add(new CashSessionMovement
            {
                // توليد معرف فريد للحركة
                Id = Guid.NewGuid(),
                // ربط الحركة بجلسة الصندوق الحالية
                CashSessionId = session.Id,
                // ربط الحركة بفاتورة المبيعات
                SalesInvoiceId = invoice.Id,
                // تحديد نوع المستند كفاتورة مبيعات
                DocumentType = FinancialDocumentType.SalesInvoice,
                // تعيين معرف المستند
                DocumentId = invoice.Id,
                // تعيين معرف الحركة من جانب العميل لضمان المطابقة
                ClientMovementId = payment.ClientMovementId,
                // تحديد وسيلة الدفع (نقدي، بطاقة، إلخ)
                PaymentMethod = payment.PaymentMethod,
                // تحديد المبلغ المحصل الفعلي
                Amount = payment.Amount,
                // تحديد المبلغ المقدم من العميل قبل إرجاع الباقي
                TenderedAmount = payment.TenderedAmount,
                // اتجاه الحركة وارد للصندوق
                Direction = TransactionDirection.Inbound
            });
        }

        // وسم خاصية حالة الجلسة كمعدلة لإثارة تعارض التزامن في حال محاولة إغلاق الوردية بالتزامن
        db.Entry(session).Property(x => x.Status).IsModified = true;

        // إرجاع مهمة مكتملة
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    /// <summary>
    /// عكس ورد مبالغ الدفع لمرتجع مبيعات والتحقق من كفاية مبالغ الفاتورة وفتح وردية في نفس الدرج.
    /// </summary>
    public async Task<ServiceResult> ReverseAsync(
        SalesInvoice invoice,
        FinancialDocumentType documentType,
        Guid documentId,
        decimal amount,
        CancellationToken ct)
    {
        // إذا كان مبلغ الرد صفراً تنتهي العملية بنجاح دون أي حركة
        if (amount == 0) return ServiceResult.Success();

        // جلب حركات القبض الأصلية للفاتورة
        var originals = await db.CashSessionMovements
            // تصفية الحركات الأصلية الواردة للفاتورة
            .Where(x => x.SalesInvoiceId == invoice.Id && x.DocumentType == FinancialDocumentType.SalesInvoice && x.Direction == TransactionDirection.Inbound)
            // الترتيب حسب وسيلة الدفع ثم المعرف
            .OrderBy(x => x.PaymentMethod).ThenBy(x => x.Id)
            // جلب البيانات
            .ToListAsync(ct);

        // التحقق من أن المبلغ المتبقي القابل للرد في الفاتورة يغطي المبلغ المطلوب رده
        if (originals.Sum(x => x.Amount - x.ReversedAmount) < amount)
        {
            // إرجاع خطأ بتجاوز قيمة الرد للرصيد المتاح
            return Fail("قيمة الرد تتجاوز المبلغ المقبوض المتاح من الفاتورة");
        }

        // جلب الجلسة المصدرية التي تمت فيها عملية القبض الأصلية
        var sourceSession = await db.CashSessions.SingleAsync(x => x.Id == originals[0].CashSessionId, ct);

        // جلب الوردية المفتوحة حالياً لنفس الدرج وبواسطة نفس الكاشير الحالي
        var active = await db.CashSessions.Include(x => x.CashDrawer).FirstOrDefaultAsync(x =>
            x.CashDrawerId == sourceSession.CashDrawerId && x.Status == CashSessionStatus.Open && x.CashierUserId == currentUser.UserId, ct);

        // التحقق من وجود وردية نشطة مفتوحة للدرج مع الكاشير الحالي
        if (active is null)
        {
            // إرجاع تنبيه بضرورة فتح وردية للدرج الأصلي أولاً
            return Fail("افتح وردية للدرج الأصلي قبل رد قيمة فاتورة الدفع المتعدد");
        }

        // قائمة لتوزيع مبالغ الرد على الحركات الأصلية
        var allocations = new List<(CashSessionMovement Original, decimal Amount)>();

        // التكرار على الحركات الأصلية لاقتطاع مبالغ الرد حتى استيفاء كامل المبلغ
        foreach (var original in originals)
        {
            // حساب الجزء المتبقي القابل للرد من هذه الحركة
            var part = Math.Min(amount, original.Amount - original.ReversedAmount);

            // في حال وجود جزء قابل للرد إضافته لقائمة التوزيع
            if (part > 0) allocations.Add((original, part));

            // خصم الجزء الموزع من إجمالي المبلغ المطلوب رده
            amount -= part;

            // التوقف عند استيفاء كامل المبلغ المطلوب رده
            if (amount == 0) break;
        }

        // حساب إجمالي الجزء النقدي المطلوب رده نقداً من الدرج
        var cash = allocations.Where(x => x.Original.PaymentMethod == PaymentMethod.Cash).Sum(x => x.Amount);

        // التحقق من صلاحية الخزينة للفرع وكفاية رصيدها للجزء النقدي الصادر
        var safe = await posting.ResolveSafeAsync(invoice.BranchId, active.CashDrawerId, cash, true, ct);

        // في حال فشل التحقق من الخزينة يتم إرجاع رسالة الخطأ
        if (!safe.IsSuccess) return Fail(safe.ErrorMessage!);

        // تسجيل حركات الرد الصادرة وتحديث المبالغ المردودة في الحركات الأصلية
        foreach (var (original, part) in allocations)
        {
            // زيادة المبلغ المردود في الحركة الأصلية لمنع تكرار رده مستقبلاً
            original.ReversedAmount += part;

            // إضافة حركة جلسة جديدة تمثل حركة صرف صادرة للمرتجع
            db.CashSessionMovements.Add(new CashSessionMovement
            {
                // معرف فريد للحركة
                Id = Guid.NewGuid(),
                // ربط الحركة بالوردية النشطة الحالية
                CashSessionId = active.Id,
                // ربط الفاتورة
                SalesInvoiceId = invoice.Id,
                // تحديد نوع المستند كمرتجع
                DocumentType = documentType,
                // تعيين معرف مستند المرتجع
                DocumentId = documentId,
                // ربط معرف حركة العميل بمعرف الحركة الأصلية
                ClientMovementId = original.Id,
                // نفس وسيلة الدفع
                PaymentMethod = original.PaymentMethod,
                // اتجاه الحركة صادر
                Direction = TransactionDirection.Outbound,
                // المبلغ المردود
                Amount = part,
                // الإشارة لمعرف الحركة الأصلية المردودة
                OriginalMovementId = original.Id
            });
        }

        // وسم حالة الوردية كمعدلة لحماية التزامن
        db.Entry(active).Property(x => x.Status).IsModified = true;

        // إذا وجد جزء نقدي يتم خصمه وترحيل رصيده من الخزينة/الدرج محاسبياً
        if (cash > 0)
        {
            // تسجيل قيد خفض رصيد الخزينة للجزء النقدي
            await posting.PostSafeBalanceAsync(
                safe.Data!,
                -cash,
                documentType,
                documentId,
                DateTime.UtcNow,
                "رد الجزء النقدي لفاتورة دفع متعدد",
                ct,
                active.Id,
                $"pos-refund:{documentType}:{documentId}");
        }

        // إرجاع نجاح العملية
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    /// <summary>
    /// التراجع عن حركات عكس مرتجع ملغى واستعادة المبالغ المقبوضة للوردية النشطة.
    /// </summary>
    public async Task<ServiceResult> UndoReturnAsync(Guid returnId, CancellationToken ct)
    {
        // جلب حركات الرد الصادرة المرتبطة بمستند المرتجع الملغى
        var refunds = await db.CashSessionMovements.Include(x => x.OriginalMovement).Include(x => x.CashSession)
            // تصفية حركات الصادر لمرتجع المبيعات
            .Where(x => x.DocumentType == FinancialDocumentType.SalesReturn && x.DocumentId == returnId && x.Direction == TransactionDirection.Outbound)
            // تنفيذ الاستعلام
            .ToListAsync(ct);

        // إذا لم توجد حركات رد تنتهي العملية بنجاح
        if (refunds.Count == 0) return ServiceResult.Success();

        // معرف الدرج النقدي المستخدم في حركات الرد
        var drawerId = refunds[0].CashSession.CashDrawerId;

        // التحقق من وجود وردية نشطة للكاشير الحالي على نفس الدرج
        var active = await db.CashSessions.FirstOrDefaultAsync(x => x.CashDrawerId == drawerId &&
            x.Status == CashSessionStatus.Open && x.CashierUserId == currentUser.UserId, ct);

        // التأكد من توفر وردية نشطة لإجراء التراجع
        if (active is null) return Fail("افتح وردية للدرج الأصلي قبل عكس المرتجع");

        // التكرار على كل حركة رد لعكسها وإعادة قيمتها
        foreach (var refund in refunds)
        {
            // التحقق من عدم عكس هذه الدفعة المرتجعة مسبقاً
            if (refund.ReversedAmount != 0) return Fail("سبق عكس هذه الدفعة المرتجعة");

            // وسم الحركة كمعكوسة بالكامل
            refund.ReversedAmount = refund.Amount;

            // إنقاص المبلغ المردود من الحركة الأصلية لاستعادة الرصيد المتاح
            refund.OriginalMovement!.ReversedAmount -= refund.Amount;

            // تسجيل حركة وارد جديدة في الوردية النشطة تمثل استرجاع النقدية
            db.CashSessionMovements.Add(new CashSessionMovement
            {
                Id = Guid.NewGuid(),
                CashSessionId = active.Id,
                SalesInvoiceId = refund.SalesInvoiceId,
                DocumentType = FinancialDocumentType.SalesReturn,
                DocumentId = returnId,
                ClientMovementId = refund.Id,
                OriginalMovementId = refund.Id,
                PaymentMethod = refund.PaymentMethod,
                Direction = TransactionDirection.Inbound,
                Amount = refund.Amount
            });
        }

        // وسم حالة الوردية كمعدلة لحماية التزامن
        db.Entry(active).Property(x => x.Status).IsModified = true;

        // حساب إجمالي الجزء النقدي المعكوس
        var cash = refunds.Where(x => x.PaymentMethod == PaymentMethod.Cash).Sum(x => x.Amount);

        // في حال وجود جزء نقدي يتم إعادة إيداعه في رصيد الدرج
        if (cash > 0)
        {
            // مطابقة الخزينة للدرج
            var safe = await posting.ResolveSafeAsync(active.BranchId, drawerId, cash, false, ct);

            // التحقق من نجاح مطابقة الخزينة
            if (!safe.IsSuccess) return Fail(safe.ErrorMessage!);

            // تسجيل قيد زيادة رصيد الخزينة بالمبلغ المستعاد
            await posting.PostSafeBalanceAsync(
                safe.Data!,
                cash,
                FinancialDocumentType.SalesReturn,
                returnId,
                DateTime.UtcNow,
                "عكس الرد النقدي لمرتجع دفع متعدد",
                ct,
                active.Id,
                $"pos-refund:{returnId}:undo");
        }

        // إرجاع نجاح العملية
        return ServiceResult.Success();
    }

    /// <inheritdoc />
    /// <summary>
    /// جلب الورديات وحركاتها المالية لفرع معين مع حصر الوصول لفرع المستخدم الحالي.
    /// </summary>
    public async Task<ServiceResult<List<CashSessionPaymentsDto>>> GetSessionsAsync(Guid? branchId, CancellationToken ct)
    {
        // استعلام معرف فرع المستخدم الحالي
        var userBranch = await db.Users.Where(x => x.Id == currentUser.UserId).Select(x => x.BranchId).FirstOrDefaultAsync(ct);

        // إذا كان المستخدم مقيداً بفرع محدد يتم إلزام الفلترة بفرعه فقط
        if (userBranch != Guid.Empty) branchId = userBranch;

        // استعلام آخر 100 وردية تحتوي على حركات مدفوعات تابعة للفرع
        var sessions = await db.CashSessions.AsNoTracking().Include(x => x.PosTerminal).Include(x => x.CashDrawer)
            // تصفية الورديات حسب الفرع والتأكد من احتوائها على حركات
            .Where(x => (!branchId.HasValue || x.BranchId == branchId) && db.CashSessionMovements.Any(m => m.CashSessionId == x.Id))
            // الترتيب من الأحدث للأقدم
            .OrderByDescending(x => x.OpenedAt)
            // أخذ آخر 100 جلسة
            .Take(100)
            // جلب البيانات
            .ToListAsync(ct);

        // استخراج معرفات الجلسات المسترجعة
        var ids = sessions.Select(x => x.Id).ToList();

        // جلب كافة حركات الجلسات المسترجعة وتفاصيل فواتيرها وخزائن تسويتها
        var movements = await db.CashSessionMovements.AsNoTracking().Include(x => x.SalesInvoice).Include(x => x.SettlementSafe)
            // تصفية الحركات التابعة للجلسات المحددة
            .Where(x => ids.Contains(x.CashSessionId))
            // الترتيب زمني حسب تاريخ الإنشاء
            .OrderBy(x => x.CreatedAt)
            // جلب القائمة
            .ToListAsync(ct);

        // تجميع وتوصيف كل جلسة مع حركاتها وإرجاع النتيجة
        return ServiceResult<List<CashSessionPaymentsDto>>.Success(
            sessions.Select(s => Describe(s, movements.Where(x => x.CashSessionId == s.Id).ToList())).ToList());
    }

    /// <inheritdoc />
    /// <summary>
    /// تسوية وتوزيع مدفوعات وردية كاشير مغلقة وترحيل صافي التحصيل للخزائن والحسابات المصرفية المقابلة.
    /// </summary>
    public async Task<ServiceResult<CashSessionPaymentsDto>> SettleAsync(
        Guid sessionId,
        SettleCashSessionPaymentsDto dto,
        CancellationToken ct)
    {
        // دالة محلية مساعدة لبناء نتيجة إخفاق مع كود الخطأ الافتراضي
        ServiceResult<CashSessionPaymentsDto> Failure(string message, string? code = null) =>
            ServiceResult<CashSessionPaymentsDto>.Failure(message, code ?? ErrorCodes.ValidationError);

        // التحقق من امتلاك المستخدم لصلاحية التحويل بين الخزائن والتسوية
        if (!permissions.HasPermission(Permissions.Safes.Transfer))
            return Failure("لا تملك صلاحية تسوية حركات الوردية", ErrorCodes.Forbidden);

        // التحقق من صحة بيانات النموذج المدخل
        if (DtoValidation.GetError(dto) is { } error) return Failure(error);

        // التأكد من عدم تكرار تحديد وسيلة الدفع لأكثر من حساب
        if (dto.Accounts.Select(x => x.PaymentMethod).Distinct().Count() != dto.Accounts.Count)
            return Failure("حدد حساباً واحداً لكل وسيلة");

        // جلب بيانات الجلسة مع درجها النقدية ونقطة البيع
        var session = await db.CashSessions.Include(x => x.CashDrawer).Include(x => x.PosTerminal).SingleOrDefaultAsync(x => x.Id == sessionId, ct);

        // التحقق من وجود الجلسة
        if (session is null) return Failure("الوردية غير موجودة", ErrorCodes.NotFound);

        // استخراج فرع المستخدم والتأكد من مطابقة فرع الجلسة لفرع المستخدم
        var userBranch = await db.Users.Where(x => x.Id == currentUser.UserId).Select(x => x.BranchId).FirstOrDefaultAsync(ct);
        if (userBranch != Guid.Empty && userBranch != session.BranchId)
            return Failure("الوردية لا تتبع فرعك", ErrorCodes.Forbidden);

        // حساب بصمة SHA256 للطلب لضمان ثبات المعالجة Idempotency عند إعادة المحاولة
        var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new
        {
            Accounts = dto.Accounts.OrderBy(x => x.PaymentMethod).Select(x => new { x.PaymentMethod, x.SafeId }),
            dto.Notes
        }))));

        // الاستعلام عن وجود تسوية سابقة مسجلة لنفس الوردية
        var existing = await db.CashSessionPaymentSettlements.AsNoTracking().SingleOrDefaultAsync(x => x.CashSessionId == sessionId, ct);

        // جلب حركات الوردية المفتوحة للتسوية
        var movements = await db.CashSessionMovements.Include(x => x.SalesInvoice).Include(x => x.OriginalMovement)
            .Where(x => x.CashSessionId == sessionId).ToListAsync(ct);

        // إذا كانت التسوية مسجلة مسبقاً بنفس البصمة نرجع النتيجة بنجاح دون تكرار الترحيل
        if (existing is not null)
        {
            return existing.RequestHash == hash
                ? ServiceResult<CashSessionPaymentsDto>.Success(Describe(session, movements))
                : Failure("الوردية مسواة بالفعل بحسابات مختلفة");
        }

        // التسوية مسموحة فقط بعد إغلاق الوردية واعتماد فروقات الجرد
        if (session.Status != CashSessionStatus.Closed)
            return Failure("التسوية متاحة بعد إغلاق الوردية واعتماد فرق الجرد فقط");

        // التأكد من وجود حركات دفع في الوردية
        if (movements.Count == 0) return Failure("لا توجد حركات دفع معلقة لهذه الوردية");

        // استخراج وسائل الدفع المستخدمة في الوردية
        var methods = movements.Select(x => x.PaymentMethod).Distinct().ToHashSet();

        // التحقق من أن طلب التسوية يغطي كافة وسائل الدفع المستخدمة
        if (!methods.SetEquals(dto.Accounts.Select(x => x.PaymentMethod)))
            return Failure("حدد حساب التسوية لكل وسيلة مستخدمة في الوردية");

        // قاموس لتخزين الخزائن المستهدفة لكل وسيلة دفع
        var targets = new Dictionary<PaymentMethod, Safe>();

        // التحقق من صحة الخزائن والحسابات المصرفية المحددة
        foreach (var account in dto.Accounts)
        {
            // جلب الخزينة المحددة
            var target = await uow.Accounting.GetSafeByIdAsync(account.SafeId, ct);

            // التحقق من وجود الخزينة ونشاطها وتبعيتها لنفس الفرع ونوعها المناسب
            if (target is null || !target.IsActive || target.BranchId != session.BranchId ||
                target.Type != (account.PaymentMethod == PaymentMethod.Cash ? SafeType.CashBox : SafeType.Bank))
            {
                return Failure("اختر صندوقاً نقدياً للنقد وحساباً مصرفياً لباقي الوسائل من نفس الفرع");
            }

            // حفظ الخزينة في القاموس
            targets[account.PaymentMethod] = target;
        }

        // فحص حركات الرد للتأكد من تسوية الحركات الأصلية أولاً
        foreach (var movement in movements.Where(x => x.OriginalMovementId.HasValue))
        {
            // استخراج الحركة الأصلية
            var original = movement.OriginalMovement!;

            // التأكد من تسوية وردية الحركة الأصلية أولاً إن كانت في وردية مختلفة
            if (original.CashSessionId != sessionId && !original.SettlementId.HasValue)
                return Failure("سوّ وردية الحركة الأصلية أولاً قبل تسوية ردها");

            // التأكد من استخدام نفس حساب التسوية الأصلي للحركة
            if (original.SettlementSafeId.HasValue && original.SettlementSafeId != targets[movement.PaymentMethod].Id)
                return Failure("يجب أن يستخدم الرد حساب التسوية الأصلي للوسيلة");
        }

        // تجميع صافي المبالغ لكل وسيلة دفع (الوارد - الصادر)
        var totals = movements.GroupBy(x => x.PaymentMethod).ToDictionary(g => g.Key, g => g.Sum(Signed));

        // التحقق من كفاية أرصدة الخزائن والحسابات المصرفية عند تجميع عدة وسائل على نفس الحساب
        foreach (var target in targets.Values.DistinctBy(x => x.Id))
        {
            // فحص الرصيد بعد تطبيق التغيير الصافي
            if (target.CurrentBalance + totals.Where(x => targets[x.Key].Id == target.Id).Sum(x => x.Value) < 0)
                return Failure("رصيد حساب التسوية لا يكفي لصافي المرتجعات");
        }

        // استخراج إجمالي التحصيل النقدي
        var cashTotal = totals.GetValueOrDefault(PaymentMethod.Cash);

        // التحقق من كفاية رصيد الدرج النقدي لتحويل المبالغ النقدية
        if (session.CashDrawer.CurrentBalance - cashTotal < 0)
            return Failure("رصيد الدرج الحالي لا يكفي لتحويل التحصيل النقدي");

        // مطابقة جلسة الترحيل الخاصة بالدرج
        var drawerSession = await pos.ResolveDrawerPostingSessionAsync(session.CashDrawerId, ct);
        if (!drawerSession.IsSuccess) return Failure(drawerSession.ErrorMessage!);

        // بدء معاملة قاعدة بيانات لحفظ التسوية وترحيل القيود
        await uow.BeginTransactionAsync(ct);

        try
        {
            // إنشاء سجل التسوية الجديد
            var settlement = new CashSessionPaymentSettlement
            {
                Id = Guid.NewGuid(),
                CashSessionId = sessionId,
                RequestHash = hash,
                Notes = dto.Notes?.Trim()
            };

            // إضافة سجل التسوية لسياق البيانات
            db.CashSessionPaymentSettlements.Add(settlement);

            // وسم حالة الوردية كمعدلة لحماية التزامن
            db.Entry(session).Property(x => x.Status).IsModified = true;

            // ترحيل المبالغ الصافية لكل خزينة/حساب مصرفي مستهدف لمنع الأرصدة السالبة المؤقتة
            foreach (var target in targets.Values.DistinctBy(x => x.Id))
            {
                // حساب الفارق الصافي التراكمي للخزينة
                var delta = totals.Where(x => targets[x.Key].Id == target.Id).Sum(x => x.Value);

                // تسجيل قيد ترحيل الرصيد للخزينة المستهدفة
                await posting.PostSafeBalanceAsync(
                    target,
                    delta,
                    FinancialDocumentType.CashSessionClosing,
                    settlement.Id,
                    settlement.SettledAt,
                    "تسوية اختيارية لتحصيل وسائل الدفع في الوردية",
                    ct,
                    idempotencyKey: $"pos-settlement:{sessionId}:target:{target.Id}");
            }

            // في حال وجود تحصيل نقدي يتم خصمه وترحيله من الدرج النقدي
            if (cashTotal != 0)
            {
                // تسجيل قيد خصم التحصيل النقدي من الدرج
                await posting.PostSafeBalanceAsync(
                    session.CashDrawer,
                    -cashTotal,
                    FinancialDocumentType.CashSessionClosing,
                    settlement.Id,
                    settlement.SettledAt,
                    "تحويل صافي التحصيل النقدي للوردية",
                    ct,
                    drawerSession.Data,
                    $"pos-settlement:{sessionId}:cash");
            }

            // وسم كافة حركات الوردية بمعرف التسوية ومعرف الخزينة المستهدفة
            foreach (var movement in movements)
            {
                movement.SettlementId = settlement.Id;
                movement.SettlementSafeId = targets[movement.PaymentMethod].Id;
            }

            // حفظ التغييرات في قاعدة البيانات
            await uow.SaveChangesAsync(ct);

            // اعتماد وتأكيد المعاملة
            await uow.CommitTransactionAsync(ct);

            // إرجاع كائن الاستجابة بعد اكتمال التسوية
            return ServiceResult<CashSessionPaymentsDto>.Success(Describe(session, movements));
        }
        catch (DbUpdateException)
        {
            // في حال حدوث تعارض تزامن يتم التراجع عن المعاملة وإرجاع خطأ التزامن
            await uow.RollbackTransactionAsync(CancellationToken.None);
            return Failure("تغيرت الوردية أو الأرصدة أثناء التسوية؛ أعد تحميل النتيجة قبل المحاولة", ErrorCodes.ConcurrencyError);
        }
        catch
        {
            // التراجع عن المعاملة عند أي استثناء آخر غير متوقع
            await uow.RollbackTransactionAsync(CancellationToken.None);
            throw;
        }
    }

    /// <summary>
    /// حساب القيمة الرقمية للحركة بإشارة موجبة للوارد وسالبة للصادر.
    /// </summary>
    /// <param name="movement">حركة جلسة الصندوق</param>
    /// <returns>المبلغ مع الإشارة المحاسبية المناسبة</returns>
    private static decimal Signed(CashSessionMovement movement) =>
        movement.Direction == TransactionDirection.Inbound ? movement.Amount : -movement.Amount;

    /// <summary>
    /// بناء نتيجة خطأ موحدة لعمليات الخدمة.
    /// </summary>
    /// <param name="message">نص رسالة الخطأ</param>
    /// <returns>كائن نتيجة خدمة يحمل حالة الفشل</returns>
    private static ServiceResult Fail(string message) =>
        ServiceResult.Failure(message, ErrorCodes.ValidationError);

    /// <summary>
    /// تجميع وتجهيز كائن DTO لوصف الوردية وإحصائيات مدفوعاتها وحركاتها المفصلة.
    /// </summary>
    /// <param name="s">كيان جلسة الصندوق</param>
    /// <param name="movements">قائمة حركات الدفع المرتبطة بالجلسة</param>
    /// <returns>كائن DTO ملخص ومفصل للجلسة ومدفوعاتها</returns>
    private static CashSessionPaymentsDto Describe(CashSession s, List<CashSessionMovement> movements) => new()
    {
        Id = s.Id,
        BranchId = s.BranchId,
        TerminalName = s.PosTerminal.Name,
        CashDrawerName = s.CashDrawer.Name,
        OpenedAt = s.OpenedAt,
        Status = s.Status,
        IsSettled = movements.Count > 0 && movements.All(x => x.SettlementId.HasValue),
        Totals = movements.GroupBy(x => x.PaymentMethod).Select(g => new CashSessionPaymentTotalDto
        {
            PaymentMethod = g.Key,
            Amount = g.Sum(Signed),
            SafeId = g.First().SettlementSafeId,
            SafeName = g.First().SettlementSafe?.Name
        }).ToList(),
        Movements = movements.Select(x => new CashSessionMovementDto
        {
            Id = x.Id,
            InvoiceNumber = x.SalesInvoice.InvoiceNumber,
            PaymentMethod = x.PaymentMethod,
            Direction = x.Direction,
            Amount = x.Amount,
            IsSettled = x.SettlementId.HasValue
        }).ToList()
    };
}
