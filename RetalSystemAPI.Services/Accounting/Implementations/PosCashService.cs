using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using RetalSystemAPI.DataAccess.Context;
using RetalSystemAPI.DataAccess.Services;
using RetalSystemAPI.Models.Accounting;
using RetalSystemAPI.Models.DTOs.Accounting.Pos;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Services.Accounting.Interfaces;
using RetalSystemAPI.Services.Common.Models;

namespace RetalSystemAPI.Services.Accounting.Implementations;

/// <summary>
/// تنفيذ خدمة إدارة نقاط البيع (POS) وحركات الصندوق والورديات النقدية وإقران أجهزة الكاشير واعتماد الفروقات.
/// </summary>
public class PosCashService : IPosCashService
{
    // سياق قاعدة بيانات الكيان للتفاعل مع جداول نقاط البيع والورديات والخزائن
    private readonly AppDbContext _db;

    // خدمة جلب بيانات المستخدم الحالي المصادق عليه
    private readonly ICurrentUserService _currentUser;

    /// <summary>
    /// باني فئة خدمة نقاط البيع وحقن سياق البيانات وخدمة المستخدم الحالي.
    /// </summary>
    /// <param name="db">سياق قاعدة البيانات</param>
    /// <param name="currentUser">خدمة المستخدم الحالي</param>
    public PosCashService(AppDbContext db, ICurrentUserService currentUser)
    {
        // تعيين سياق قاعدة البيانات
        _db = db;
        // تعيين مرجع المستخدم الحالي
        _currentUser = currentUser;
    }

    /// <inheritdoc />
    /// <summary>
    /// استرجاع كافة نقاط البيع مع بيانات فروعها وأدراج النقدية الافتراضية.
    /// </summary>
    public async Task<ServiceResult<IReadOnlyList<PosTerminalResponseDto>>> GetTerminalsAsync(CancellationToken ct = default)
    {
        // استعلام جلب نقاط البيع بدون تتبع مع تحميل الفرع والدرج وترتيبها حسب الاسم
        var terminals = await _db.PosTerminals.AsNoTracking()
            // تضمين بيانات الفرع المرتبط
            .Include(x => x.Branch)
            // تضمين بيانات درج النقدية الافتراضي
            .Include(x => x.DefaultCashDrawer)
            // الترتيب أبجدياً حسب اسم نقطة البيع
            .OrderBy(x => x.Name)
            // تنفيذ الاستعلام وتحويل النتائج لقائمة غير متزامنة
            .ToListAsync(ct);

        // تحويل الكيانات إلى كائنات استجابة DTO وإرجاع النتيجة بنجاح
        return ServiceResult<IReadOnlyList<PosTerminalResponseDto>>.Success(terminals.Select(MapTerminal).ToList());
    }

    /// <inheritdoc />
    /// <summary>
    /// إنشاء نقطة بيع جديدة بعد التحقق من صحة الفرع والدرج وعدم تكرار الكود والاسم.
    /// </summary>
    public async Task<ServiceResult<PosTerminalResponseDto>> CreateTerminalAsync(CreatePosTerminalDto dto, CancellationToken ct = default)
    {
        // التحقق من صحة بيانات نقطة البيع وعدم تكرارها
        var validation = await ValidateTerminalAsync(dto.BranchId, dto.DefaultCashDrawerId, null, dto.Code, dto.Name, ct);
        // في حال وجود خطأ في التحقق
        if (validation != null)
            // إرجاع خطأ التحقق من الصحة
            return ServiceResult<PosTerminalResponseDto>.Failure(validation, ErrorCodes.ValidationError);

        // إنشاء كائن نقطة البيع الجديد
        var terminal = new PosTerminal
        {
            // تعيين كود نقطة البيع بعد إزالة المسافات الزائدة
            Code = dto.Code.Trim(),
            // تعيين اسم نقطة البيع
            Name = dto.Name.Trim(),
            // تعيين معرف الفرع التابعة له
            BranchId = dto.BranchId,
            // تعيين درج النقدية الافتراضي
            DefaultCashDrawerId = dto.DefaultCashDrawerId,
            // تفعيل نقطة البيع افتراضياً
            IsActive = true
        };

        // إضافة الكائن إلى سياق قاعدة البيانات
        _db.PosTerminals.Add(terminal);
        // حفظ التغييرات في قاعدة البيانات
        await _db.SaveChangesAsync(ct);
        // تحميل بيانات الفرع المرتبط بشكل صريح
        await _db.Entry(terminal).Reference(x => x.Branch).LoadAsync(ct);
        // تحميل بيانات درج النقدية المرتبط بشكل صريح
        await _db.Entry(terminal).Reference(x => x.DefaultCashDrawer).LoadAsync(ct);

        // إرجاع نقطة البيع المنشأة بنجاح
        return ServiceResult<PosTerminalResponseDto>.Success(MapTerminal(terminal));
    }

    /// <inheritdoc />
    /// <summary>
    /// تحديث بيانات نقطة بيع قائمة والتحقق من عدم وجود ورديات مفتوحة تعيق التعديل.
    /// </summary>
    public async Task<ServiceResult<PosTerminalResponseDto>> UpdateTerminalAsync(Guid id, UpdatePosTerminalDto dto, CancellationToken ct = default)
    {
        // جلب نقطة البيع المراد تعديلها مع الفرع والدرج
        var terminal = await _db.PosTerminals
            .Include(x => x.Branch)
            .Include(x => x.DefaultCashDrawer)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        // إذا لم يتم العثور على نقطة البيع
        if (terminal == null)
            // إرجاع خطأ عدم العثور على نقطة البيع
            return ServiceResult<PosTerminalResponseDto>.Failure("نقطة البيع غير موجودة", ErrorCodes.NotFound);

        // التحقق من صحة البيانات الجديدة وعدم تكرارها مع نقاط أخرى
        var validation = await ValidateTerminalAsync(dto.BranchId, dto.DefaultCashDrawerId, id, dto.Code, dto.Name, ct);
        // في حال وجود خطأ في التحقق
        if (validation != null)
            // إرجاع خطأ التحقق
            return ServiceResult<PosTerminalResponseDto>.Failure(validation, ErrorCodes.ValidationError);

        // التحقق مما إذا كانت نقطة البيع تحتوي على وردية مفتوحة أو قيد الاعتماد
        var hasActiveSession = await _db.CashSessions.AnyAsync(x => x.PosTerminalId == id && (x.Status == CashSessionStatus.Open || x.Status == CashSessionStatus.PendingApproval), ct);
        // في حال وجود وردية نشطة ومحاولة تعطيلها أو تغيير فرعها أو درجها
        if (hasActiveSession && (!dto.IsActive || dto.BranchId != terminal.BranchId || dto.DefaultCashDrawerId != terminal.DefaultCashDrawerId))
            // إرجاع خطأ منع التعديل أثناء وجود وردية مفتوحة
            return ServiceResult<PosTerminalResponseDto>.Failure("لا يمكن تعطيل نقطة البيع أو تغيير فرعها/درجها وفيها وردية مفتوحة", ErrorCodes.ValidationError);

        // تحديث كود نقطة البيع
        terminal.Code = dto.Code.Trim();
        // تحديث اسم نقطة البيع
        terminal.Name = dto.Name.Trim();
        // تحديث الفرع
        terminal.BranchId = dto.BranchId;
        // تحديث الدرج الافتراضي
        terminal.DefaultCashDrawerId = dto.DefaultCashDrawerId;
        // تحديث حالة النشاط
        terminal.IsActive = dto.IsActive;

        // حفظ التعديلات في قاعدة البيانات
        await _db.SaveChangesAsync(ct);
        // إعادة تحميل مرجع الفرع المحدث
        await _db.Entry(terminal).Reference(x => x.Branch).LoadAsync(ct);
        // إعادة تحميل مرجع درج النقدية المحدث
        await _db.Entry(terminal).Reference(x => x.DefaultCashDrawer).LoadAsync(ct);

        // إرجاع بيانات نقطة البيع بعد التحديث
        return ServiceResult<PosTerminalResponseDto>.Success(MapTerminal(terminal));
    }

    /// <inheritdoc />
    /// <summary>
    /// توليد كود اقتران عشوائي لجهاز الكاشير صالح لمدة 5 دقائق وتشفير حفظه.
    /// </summary>
    public async Task<ServiceResult<PairingCodeResponseDto>> GeneratePairingCodeAsync(Guid terminalId, CancellationToken ct = default)
    {
        // جلب نقطة البيع المحددة
        var terminal = await _db.PosTerminals.FirstOrDefaultAsync(x => x.Id == terminalId, ct);
        // إذا لم توجد نقطة البيع
        if (terminal == null)
            // إرجاع خطأ عدم العثور عليها
            return ServiceResult<PairingCodeResponseDto>.Failure("نقطة البيع غير موجودة", ErrorCodes.NotFound);
        // إذا كانت نقطة البيع معطلة
        if (!terminal.IsActive)
            // إرجاع خطأ التعطيل
            return ServiceResult<PairingCodeResponseDto>.Failure("نقطة البيع معطلة", ErrorCodes.ValidationError);

        // توليد رقم عشوائي آمن مكون من 8 أرقام
        var code = RandomNumberGenerator.GetInt32(10_000_000, 100_000_000).ToString();
        // تحديد موعد انتهاء صلاحية الكود بعد 5 دقائق
        var expiresAt = DateTime.UtcNow.AddMinutes(5);
        // تشفير كود الاقتران مع معرف المستأجر وحفظ الهاش فقط
        terminal.PairingCodeHash = Hash($"{terminal.TenantId:N}:{code}");
        // حفظ وقت انتهاء الصلاحية
        terminal.PairingCodeExpiresAt = expiresAt;
        // حفظ التغييرات في قاعدة البيانات
        await _db.SaveChangesAsync(ct);

        // إرجاع كود الاقتران ووقت انتهائه للواجهة
        return ServiceResult<PairingCodeResponseDto>.Success(new PairingCodeResponseDto
        {
            // معرف المستأجر
            TenantId = terminal.TenantId,
            // معرف نقطة البيع
            TerminalId = terminal.Id,
            // كود الاقتران الصريح
            PairingCode = code,
            // تاريخ انتهاء الصلاحية
            ExpiresAt = expiresAt
        });
    }

    /// <inheritdoc />
    /// <summary>
    /// ربط جهاز الكاشير بنقطة البيع باستخدام رمز الاقتران وتوليد توكن وصول دائم للجهاز.
    /// </summary>
    public async Task<ServiceResult<ClaimPosDeviceResponseDto>> ClaimDeviceAsync(ClaimPosDeviceDto dto, CancellationToken ct = default)
    {
        // حساب الهاش لرمز الاقتران المدخل
        var codeHash = Hash($"{dto.TenantId:N}:{dto.PairingCode.Trim()}");
        // البحث عن نقطة البيع المطابقة مع تجاهل الفلاتر لتحديد المستأجر
        var terminal = await _db.PosTerminals.IgnoreQueryFilters()
            // تضمين بيانات الفرع
            .Include(x => x.Branch)
            // تضمين بيانات الدرج
            .Include(x => x.DefaultCashDrawer)
            // شرط تطابق المستأجر والنشاط والهاش وسريان الصلاحية
            .FirstOrDefaultAsync(x => x.TenantId == dto.TenantId && !x.IsDeleted && x.IsActive &&
                x.PairingCodeHash == codeHash && x.PairingCodeExpiresAt > DateTime.UtcNow, ct);

        // إذا لم يتم العثور على نقطة بيع مطابقة
        if (terminal == null)
            // إرجاع خطأ عدم صحة الرمز أو انتهاء صلاحيته
            return ServiceResult<ClaimPosDeviceResponseDto>.Failure("رمز الربط غير صحيح أو انتهت صلاحيته", ErrorCodes.ValidationError);

        // توليد توكن عشوائي قوي بطول 32 بايت بصيغة Hex
        var rawToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(32));
        // تحديد انتهاء صلاحية الجهاز بعد 90 يوماً
        var expiresAt = DateTime.UtcNow.AddDays(90);

        // إنشاء سجل بيانات اعتماد جهاز الكاشير وحفظ الهاش
        _db.PosDeviceCredentials.Add(new PosDeviceCredential
        {
            // تعيين معرف المستأجر
            TenantId = terminal.TenantId,
            // تعيين معرف نقطة البيع
            PosTerminalId = terminal.Id,
            // حفظ التجزئة المشفرة للتوكن
            TokenHash = Hash(rawToken),
            // تاريخ انتهاء الصلاحية
            ExpiresAt = expiresAt,
            // تاريخ آخر اتصال
            LastSeenAt = DateTime.UtcNow
        });

        // تصفير رمز الاقتران بعد استخدامه بنجاح
        terminal.PairingCodeHash = null;
        // تصفير تاريخ انتهاء رمز الاقتران
        terminal.PairingCodeExpiresAt = null;
        // تحديث تاريخ آخر ظهور لنقطة البيع
        terminal.LastSeenAt = DateTime.UtcNow;
        // حفظ التغييرات في قاعدة البيانات
        await _db.SaveChangesAsync(ct);

        // إرجاع توكن الجهاز وبيانات نقطة البيع
        return ServiceResult<ClaimPosDeviceResponseDto>.Success(new ClaimPosDeviceResponseDto
        {
            // التوكن الخام المرسل للجهاز لحفظه محلياً
            DeviceToken = rawToken,
            // موعد انتهاء صلاحية التوكن
            ExpiresAt = expiresAt,
            // بيانات نقطة البيع المرتبطة
            Terminal = MapTerminal(terminal)
        });
    }

    /// <inheritdoc />
    /// <summary>
    /// استرجاع سياق نقطة البيع والوردية النشطة الحالية للمستخدم المصادق عليه.
    /// </summary>
    public async Task<ServiceResult<PosContextDto>> GetContextAsync(string? deviceToken, CancellationToken ct = default)
    {
        // التحقق من صحة جهاز الكاشير عبر التوكن
        var resolved = await ResolveDeviceAsync(deviceToken, ct);
        // في حال وجود خطأ في الجهاز
        if (resolved.Error != null)
            // إرجاع خطأ التحقق
            return ServiceResult<PosContextDto>.Failure(resolved.Error, ErrorCodes.ValidationError);

        // استخراج كائن نقطة البيع المعتمد
        var terminal = resolved.Terminal!;
        // استخراج بيانات الاعتماد
        var credential = resolved.Credential!;
        // جلب الوردية النشطة المفتوحة لنقطة البيع والمستخدم الحالي
        var session = await ActiveSessionQuery().FirstOrDefaultAsync(x => x.PosTerminalId == terminal.Id && x.CashierUserId == _currentUser.UserId, ct);

        // تحديث وقت آخر ظهور لنقطة البيع
        terminal.LastSeenAt = DateTime.UtcNow;
        // تحديث وقت آخر ظهور لاعتماد الجهاز
        credential.LastSeenAt = DateTime.UtcNow;
        // حفظ التحديثات
        await _db.SaveChangesAsync(ct);

        // إرجاع سياق نقطة البيع مع بيانات الوردية إن وجدت
        return ServiceResult<PosContextDto>.Success(new PosContextDto
        {
            // بيانات نقطة البيع
            Terminal = MapTerminal(terminal),
            // بيانات الوردية الحالية
            CurrentSession = session == null ? null : MapSession(session)
        });
    }

    /// <inheritdoc />
    /// <summary>
    /// فتح وردية صندوق جديدة والتأكد من مطابقة رصيد الافتتاح للرصيد الفعلي المسجل للدرج.
    /// </summary>
    public async Task<ServiceResult<CashSessionResponseDto>> OpenSessionAsync(string? deviceToken, OpenCashSessionDto dto, CancellationToken ct = default)
    {
        // مطابقة جهاز الكاشير عبر التوكن
        var resolved = await ResolveDeviceAsync(deviceToken, ct);
        // في حال فشل التحقق من الجهاز
        if (resolved.Error != null)
            // إرجاع خطأ الجهاز
            return ServiceResult<CashSessionResponseDto>.Failure(resolved.Error, ErrorCodes.ValidationError);

        // استخراج نقطة البيع
        var terminal = resolved.Terminal!;
        // جلب بيانات المستخدم والكاشير الحالي من قاعدة البيانات
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == _currentUser.UserId, ct);
        // التحقق من وجود المستخدم وتبعيتة لنفس فرع نقطة البيع
        if (user == null || user.BranchId != terminal.BranchId)
            // إرجاع خطأ عدم تبعية المستخدم للفرع
            return ServiceResult<CashSessionResponseDto>.Failure("المستخدم لا يتبع فرع نقطة البيع", ErrorCodes.Forbidden);

        // التحقق من تطابق الرصيد الافتتاحي المعدود مع رصيد الدرج الدفتري
        if (Math.Abs(terminal.DefaultCashDrawer.CurrentBalance - dto.OpeningFloat) > 0.0001m)
            // إرجاع خطأ عدم تطابق رصيد البداية مع رصيد الدرج
            return ServiceResult<CashSessionResponseDto>.Failure($"الرصيد المعدود ({dto.OpeningFloat:N2}) لا يطابق رصيد الدرج المسجل ({terminal.DefaultCashDrawer.CurrentBalance:N2})", ErrorCodes.ValidationError);

        // التحقق من عدم وجود وردية مفتوحة مسبقاً لهذا الدرج أو لنفس الكاشير
        if (await ActiveSessionQuery().AnyAsync(x => x.CashDrawerId == terminal.DefaultCashDrawerId || x.CashierUserId == user.Id, ct))
            // إرجاع خطأ وجود وردية مفتوحة بالفعل
            return ServiceResult<CashSessionResponseDto>.Failure("يوجد بالفعل وردية مفتوحة لهذا الدرج أو لهذا الكاشير", ErrorCodes.ValidationError);

        // إنشاء كيان الوردية النقدية الجديدة
        var session = new CashSession
        {
            // معرف الفرع
            BranchId = terminal.BranchId,
            // معرف نقطة البيع
            PosTerminalId = terminal.Id,
            // معرف درج النقدية
            CashDrawerId = terminal.DefaultCashDrawerId,
            // معرف الكاشير
            CashierUserId = user.Id,
            // تاريخ ووقت فتح الوردية
            OpenedAt = DateTime.UtcNow,
            // رصيد بداية الوردية المعدود
            OpeningFloat = dto.OpeningFloat,
            // الرصيد المتوقع عند الإغلاق مبدئياً
            ExpectedClosingBalance = dto.OpeningFloat,
            // تعيين حالة الوردية كمفتوحة
            Status = CashSessionStatus.Open
        };

        // إضافة الوردية إلى سياق البيانات
        _db.CashSessions.Add(session);
        // حفظ التغييرات في قاعدة البيانات
        await _db.SaveChangesAsync(ct);
        // تعيين مراجع الكائنات المحملة لخرائط DTO
        session.PosTerminal = terminal;
        // تعيين مرجع درج النقدية
        session.CashDrawer = terminal.DefaultCashDrawer;

        // إرجاع بيانات الوردية المفتوحة بنجاح
        return ServiceResult<CashSessionResponseDto>.Success(MapSession(session));
    }

    /// <inheritdoc />
    /// <summary>
    /// إغلاق الوردية النقدية ومطابقة النقدية المعدودة مع المتوقعة واكتشاف الفروقات.
    /// </summary>
    public async Task<ServiceResult<CashSessionResponseDto>> CloseSessionAsync(string? deviceToken, CloseCashSessionDto dto, CancellationToken ct = default)
    {
        // مطابقة وتوثيق جهاز الكاشير
        var resolved = await ResolveDeviceAsync(deviceToken, ct);
        // إذا وجد خطأ في الاعتماد
        if (resolved.Error != null)
            // إرجاع خطأ الاعتماد
            return ServiceResult<CashSessionResponseDto>.Failure(resolved.Error, ErrorCodes.ValidationError);

        // جلب الوردية المفتوحة التابعة لنقطة البيع والمستخدم الحالي
        var session = await ActiveSessionQuery().FirstOrDefaultAsync(x => x.PosTerminalId == resolved.Terminal!.Id && x.CashierUserId == _currentUser.UserId, ct);
        // التحقق من وجود الوردية وحالتها كمفتوحة
        if (session == null || session.Status != CashSessionStatus.Open)
            // إرجاع خطأ عدم وجود وردية قابلة للإغلاق
            return ServiceResult<CashSessionResponseDto>.Failure("لا توجد وردية مفتوحة قابلة للإغلاق", ErrorCodes.ValidationError);

        // حساب صافي الحركات النقدية التي تمت أثناء هذه الوردية (واردة - صادرة)
        var movement = await _db.SafeTransactions.Where(x => x.CashSessionId == session.Id)
            // جمع الحركات الواردة وطرح الصادرة
            .SumAsync(x => x.Direction == TransactionDirection.Inbound ? x.Amount : -x.Amount, ct);

        // حساب الرصيد المتوقع إغلاقاً (الافتتاحي + صافي الحركة)
        session.ExpectedClosingBalance = session.OpeningFloat + movement;
        // تسجيل الرصيد الفعلي المعدود بواسطة الكاشير
        session.CountedClosingBalance = dto.CountedClosingBalance;
        // احتساب فرق الجرد (الفعلي - المتوقع)
        session.Difference = dto.CountedClosingBalance - session.ExpectedClosingBalance;
        // تسجيل ملاحظات الإغلاق إن وجدت
        session.ClosingNotes = dto.Notes?.Trim();

        // في حال تطابق الأرصدة (لا يوجد فرق أو أقل من الكسر المسموح)
        if (Math.Abs(session.Difference.Value) <= 0.0001m)
        {
            // إغلاق الوردية مباشرة كمعتمدة
            session.Status = CashSessionStatus.Closed;
            // تسجيل وقت الإغلاق
            session.ClosedAt = DateTime.UtcNow;
        }
        else
        {
            // في حال وجود فرق (عجز أو فائض) تحويل الوردية لانتظار اعتماد المشرف
            session.Status = CashSessionStatus.PendingApproval;
        }

        // حفظ تغييرات إغلاق الوردية
        await _db.SaveChangesAsync(ct);

        // إرجاع بيانات الوردية المغلقة
        return ServiceResult<CashSessionResponseDto>.Success(MapSession(session));
    }

    /// <inheritdoc />
    /// <summary>
    /// اعتماد الفروقات النقدية للوردية من قبل المشرف وترحيل قيد تسوية الدرج المالي.
    /// </summary>
    public async Task<ServiceResult<CashSessionResponseDto>> ApproveDifferenceAsync(Guid sessionId, ApproveCashDifferenceDto dto, CancellationToken ct = default)
    {
        // جلب الوردية المطلوب اعتمادها مع نقطة البيع والدرج
        var session = await _db.CashSessions
            // تضمين بيانات نقطة البيع
            .Include(x => x.PosTerminal)
            // تضمين بيانات درج النقدية
            .Include(x => x.CashDrawer)
            // البحث عن الوردية بالمعرف
            .FirstOrDefaultAsync(x => x.Id == sessionId, ct);

        // إذا لم توجد الوردية
        if (session == null)
            // إرجاع خطأ عدم وجود الوردية
            return ServiceResult<CashSessionResponseDto>.Failure("الوردية غير موجودة", ErrorCodes.NotFound);

        // التحقق من أن الوردية في حالة انتظار الاعتماد ولديها فرق مسجل
        if (session.Status != CashSessionStatus.PendingApproval || !session.Difference.HasValue)
            // إرجاع خطأ عدم حاجة الوردية للاعتماد
            return ServiceResult<CashSessionResponseDto>.Failure("الوردية لا تنتظر اعتماد فرق جرد", ErrorCodes.ValidationError);

        // استخراج قيمة الفرق المسجل
        var difference = session.Difference.Value;
        // إذا كان الفرق عجزاً وكان رصيد الدرج لا يكفي لتسجيل العجز
        if (difference < 0 && session.CashDrawer.CurrentBalance < Math.Abs(difference))
            // إرجاع خطأ عدم كفاية رصيد الدرج
            return ServiceResult<CashSessionResponseDto>.Failure("رصيد الدرج غير كافٍ لتسجيل فرق العجز", ErrorCodes.InsufficientSafeBalance);

        // بدء معاملة مصرفية ذرية لضمان سلامة الترحيل
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        // تعديل رصيد الدرج الحالي بقيمة الفرق
        session.CashDrawer.CurrentBalance += difference;

        // إضافة حركة تسوية نقدية في جدول حركات الخزينة
        _db.SafeTransactions.Add(new SafeTransaction
        {
            // توليد رقم حركة تسوية فريد
            TransactionNumber = $"VAR-{DateTime.UtcNow:yyyyMMddHHmmssfff}",
            // معرف الدرج
            SafeId = session.CashDrawerId,
            // اتجاه الحركة (وارد للفائض، صادر للعجز)
            Direction = difference >= 0 ? TransactionDirection.Inbound : TransactionDirection.Outbound,
            // قيمة المبلغ المطلقة
            Amount = Math.Abs(difference),
            // الرصيد بعد التسوية
            BalanceAfter = session.CashDrawer.CurrentBalance,
            // تاريخ الحركة
            TransactionDate = DateTime.UtcNow,
            // نوع المستند حركة تسوية نقدية
            DocumentType = FinancialDocumentType.CashVariance,
            // معرف المستند
            DocumentId = session.Id,
            // معرف الوردية
            CashSessionId = session.Id,
            // معرف المستخدم المشرف المعتمد
            PerformedByUserId = _currentUser.UserId,
            // مفتاح منع تكرار التسوية
            IdempotencyKey = $"cash-session:{session.Id}:variance",
            // بيان ووصف حركة الفروقات
            Description = $"فرق جرد وردية معتمد: {dto.Notes.Trim()}"
        });

        // تغيير حالة الوردية إلى مغلقة
        session.Status = CashSessionStatus.Closed;
        // تسجيل وقت الإغلاق
        session.ClosedAt = DateTime.UtcNow;
        // تسجيل معرف المشرف المعتمد
        session.ApprovedByUserId = _currentUser.UserId;
        // تسجيل وقت الاعتماد
        session.ApprovedAt = DateTime.UtcNow;

        // حفظ التغييرات في قاعدة البيانات
        await _db.SaveChangesAsync(ct);
        // تأكيد نجاح المعاملة الذرية
        await transaction.CommitAsync(ct);

        // إرجاع بيانات الوردية المعتمدة
        return ServiceResult<CashSessionResponseDto>.Success(MapSession(session));
    }

    /// <inheritdoc />
    /// <summary>
    /// مطابقة والتحقق من صلاحية وردية مفتوحة للترحيل المالي لمستند مبيعات أو سند نقدية.
    /// </summary>
    public async Task<ServiceResult<CashSession>> ResolveOpenSessionForPostingAsync(Guid sessionId, Guid branchId, CancellationToken ct = default)
    {
        // جلب الوردية مع الدرج ونقطة البيع
        var session = await _db.CashSessions
            // تضمين بيانات الدرج
            .Include(x => x.CashDrawer)
            // تضمين بيانات نقطة البيع
            .Include(x => x.PosTerminal)
            // البحث عن الوردية بالمعرف
            .FirstOrDefaultAsync(x => x.Id == sessionId, ct);

        // التحقق من وجود الوردية وكونها مفتوحة
        if (session == null || session.Status != CashSessionStatus.Open)
            // إرجاع خطأ أن الوردية غير موجودة أو مغلقة
            return ServiceResult<CashSession>.Failure("وردية درج النقود غير موجودة أو ليست مفتوحة", ErrorCodes.ValidationError);

        // التحقق من أن الوردية تتبع نفس الفرع ونفس الكاشير الحالي
        if (session.BranchId != branchId || session.CashierUserId != _currentUser.UserId)
            // إرجاع خطأ عدم الصلاحية
            return ServiceResult<CashSession>.Failure("الوردية لا تخص المستخدم أو الفرع الحالي", ErrorCodes.Forbidden);

        // التحقق من أن الدرج نشط ومن نوع درج نقود
        if (!session.CashDrawer.IsActive || session.CashDrawer.Type != SafeType.CashDrawer)
            // إرجاع خطأ عدم صلاحية الدرج
            return ServiceResult<CashSession>.Failure("درج النقود المرتبط بالوردية غير صالح", ErrorCodes.ValidationError);

        // إرجاع الوردية المفتوحة المعتمدة للترحيل
        return ServiceResult<CashSession>.Success(session);
    }

    /// <inheritdoc />
    /// <summary>
    /// استرجاع معرف الوردية النقدية النشطة المرتبطة بدرج محدد لإتمام الترحيل التلقائي.
    /// </summary>
    public async Task<ServiceResult<Guid?>> ResolveDrawerPostingSessionAsync(Guid safeId, CancellationToken ct = default)
    {
        // البحث عن وردية نشطة مفتوحة أو بانتظار الاعتماد للدرج
        var session = await _db.CashSessions.AsNoTracking().FirstOrDefaultAsync(x => x.CashDrawerId == safeId &&
            (x.Status == CashSessionStatus.Open || x.Status == CashSessionStatus.PendingApproval), ct);

        // في حال كانت الوردية بانتظار الاعتماد فإن الدرج مجمد
        if (session?.Status == CashSessionStatus.PendingApproval)
            // إرجاع خطأ تجميد الدرج حتى اعتماد الفرق
            return ServiceResult<Guid?>.Failure("درج النقود مجمّد حتى يعتمد المشرف فرق جرد الوردية", ErrorCodes.ValidationError);

        // إرجاع معرف الوردية إن وجدت بنجاح
        return ServiceResult<Guid?>.Success(session?.Id);
    }

    /// <inheritdoc />
    /// <summary>
    /// تدقيق القيود لمنع تعطيل أو تغيير نوع درج نقود مرتبط بنقاط بيع أو ورديات نشطة.
    /// </summary>
    public async Task<ServiceResult<bool>> ValidateSafeUpdateAsync(Guid safeId, SafeType newType, bool isActive, CancellationToken ct = default)
    {
        // التحقق مما إذا كان الدرج معيناً كنقطة بيع افتراضية
        var assigned = await _db.PosTerminals.AnyAsync(x => x.DefaultCashDrawerId == safeId, ct);
        // في حال كان معيناً وتم محاولة تغيير نوعه من درج نقود
        if (assigned && newType != SafeType.CashDrawer)
            // إرجاع خطأ منع تغيير نوع الدرج
            return ServiceResult<bool>.Failure("لا يمكن تغيير نوع درج نقود مرتبط بنقطة بيع", ErrorCodes.ValidationError);

        // التحقق مما إذا كان الدرج مرتبطاً بورديات مفتوحة أو قيد الاعتماد
        var hasSession = await _db.CashSessions.AnyAsync(x => x.CashDrawerId == safeId &&
            (x.Status == CashSessionStatus.Open || x.Status == CashSessionStatus.PendingApproval), ct);
        // في حال وجود ورديات ومحاولة تعطيل الدرج
        if (hasSession && !isActive)
            // إرجاع خطأ منع تعطيل الدرج أثناء وجود وردية نشطة
            return ServiceResult<bool>.Failure("لا يمكن تعطيل درج نقود مرتبط بورديّة مفتوحة أو قيد الاعتماد", ErrorCodes.ValidationError);

        // إرجاع نجاح التحقق
        return ServiceResult<bool>.Success(true);
    }

    /// <summary>
    /// استعلام مساعد لجلب الورديات النشطة (المفتوحة أو التي تنتظر الاعتماد) متضمنة نقطة البيع والدرج.
    /// </summary>
    // إنشاء استعلام الورديات النشطة مع الكيانات المرتبطة
    private IQueryable<CashSession> ActiveSessionQuery() => _db.CashSessions
        // تضمين نقطة البيع
        .Include(x => x.PosTerminal)
        // تضمين درج النقدية
        .Include(x => x.CashDrawer)
        // تصفية الورديات المفتوحة أو قيد الاعتماد
        .Where(x => x.Status == CashSessionStatus.Open || x.Status == CashSessionStatus.PendingApproval);

    /// <summary>
    /// مطابقة وفحص توكن جهاز الكاشير واستخراج بيانات الاعتماد ونقطة البيع النشطة.
    /// </summary>
    // مطابقة اعتماد الجهاز من خلال التوكن المشفر
    private async Task<(PosTerminal? Terminal, PosDeviceCredential? Credential, string? Error)> ResolveDeviceAsync(string? rawToken, CancellationToken ct)
    {
        // التحقق من وجود التوكن
        if (string.IsNullOrWhiteSpace(rawToken)) return (null, null, "هذا الجهاز غير مربوط بنقطة بيع");
        // حساب هاش التوكن
        var hash = Hash(rawToken.Trim());
        // البحث عن اعتماد الجهاز في قاعدة البيانات
        var credential = await _db.PosDeviceCredentials
            // تضمين نقطة البيع والفرع
            .Include(x => x.PosTerminal).ThenInclude(x => x.Branch)
            // تضمين الدرج الافتراضي
            .Include(x => x.PosTerminal).ThenInclude(x => x.DefaultCashDrawer)
            // شرط تطابق الهاش وعدم الإلغاء وسريان الصلاحية
            .FirstOrDefaultAsync(x => x.TokenHash == hash && x.RevokedAt == null && x.ExpiresAt > DateTime.UtcNow, ct);

        // التحقق من وجود الاعتماد ونشاط نقطة البيع
        if (credential == null || !credential.PosTerminal.IsActive) return (null, null, "اعتماد الجهاز غير صالح أو منتهي");
        // إرجاع الكيانات بنجاح
        return (credential.PosTerminal, credential, null);
    }

    /// <summary>
    /// التحقق من صحة بيانات الفرع والدرج والكود والاسم عند إنشاء أو تعديل نقطة البيع.
    /// </summary>
    // التحقق من صحة معايير إنشاء أو تحديث نقطة البيع
    private async Task<string?> ValidateTerminalAsync(Guid branchId, Guid drawerId, Guid? terminalId, string code, string name, CancellationToken ct)
    {
        // التحقق من وجود الفرع ونشاطه
        if (!await _db.Branches.AnyAsync(x => x.Id == branchId && x.IsActive, ct)) return "الفرع غير موجود أو معطل";
        // التحقق من وجود درج النقدية وتبعيته للفرع
        var drawer = await _db.Safes.AsNoTracking().FirstOrDefaultAsync(x => x.Id == drawerId, ct);
        // التحقق من نوع ونشاط الدرج
        if (drawer == null || !drawer.IsActive || drawer.Type != SafeType.CashDrawer || drawer.BranchId != branchId)
            return "يجب اختيار درج نقود نشط تابع لنفس الفرع";
        // التحقق من عدم تكرار كود نقطة البيع أو اسمها في الفرع
        if (await _db.PosTerminals.AnyAsync(x => x.Id != terminalId && (x.Code == code.Trim() || (x.BranchId == branchId && x.Name == name.Trim())), ct))
            return "رمز نقطة البيع أو اسمها مستخدم مسبقاً";
        // لا توجد أخطاء
        return null;
    }

    /// <summary>
    /// حساب تجزئة SHA256 للنصوص وتوليد قيمة Hex مشفرة.
    /// </summary>
    // حساب التجزئة المشفرة SHA-256 للنص المدخل
    private static string Hash(string value) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(value)));

    /// <summary>
    /// تحويل كائن نقطة البيع إلى كائن DTO للاستجابة.
    /// </summary>
    // تحويل كيان نقطة البيع إلى كائن استجابة DTO
    private static PosTerminalResponseDto MapTerminal(PosTerminal x) => new()
    {
        // تعيين المعرف
        Id = x.Id,
        // تعيين الكود
        Code = x.Code,
        // تعيين الاسم
        Name = x.Name,
        // تعيين معرف الفرع
        BranchId = x.BranchId,
        // تعيين اسم الفرع
        BranchName = x.Branch?.Name ?? "",
        // تعيين معرف الدرج الافتراضي
        DefaultCashDrawerId = x.DefaultCashDrawerId,
        // تعيين اسم الدرج الافتراضي
        DefaultCashDrawerName = x.DefaultCashDrawer?.Name ?? "",
        // تعيين حالة النشاط
        IsActive = x.IsActive,
        // تعيين تاريخ آخر ظهور
        LastSeenAt = x.LastSeenAt
    };

    /// <summary>
    /// تحويل كائن الوردية النقدية إلى كائن DTO للاستجابة.
    /// </summary>
    // تحويل كيان الوردية إلى كائن استجابة DTO
    private static CashSessionResponseDto MapSession(CashSession x) => new()
    {
        // تعيين المعرف
        Id = x.Id,
        // تعيين معرف الفرع
        BranchId = x.BranchId,
        // تعيين معرف نقطة البيع
        PosTerminalId = x.PosTerminalId,
        // تعيين اسم نقطة البيع
        TerminalName = x.PosTerminal?.Name ?? "",
        // تعيين معرف الدرج
        CashDrawerId = x.CashDrawerId,
        // تعيين اسم الدرج
        CashDrawerName = x.CashDrawer?.Name ?? "",
        // تعيين معرف الكاشير
        CashierUserId = x.CashierUserId,
        // وقت الفتح
        OpenedAt = x.OpenedAt,
        // رصيد البداية
        OpeningFloat = x.OpeningFloat,
        // الرصيد المتوقع
        ExpectedClosingBalance = x.ExpectedClosingBalance,
        // الرصيد الفعلي المعدود
        CountedClosingBalance = x.CountedClosingBalance,
        // فرق الجرد
        Difference = x.Difference,
        // وقت الإغلاق
        ClosedAt = x.ClosedAt,
        // حالة الوردية
        Status = x.Status,
        // ملاحظات الإغلاق
        ClosingNotes = x.ClosingNotes,
        // معرف المشرف المعتمد
        ApprovedByUserId = x.ApprovedByUserId,
        // وقت الاعتماد
        ApprovedAt = x.ApprovedAt
    };
}
