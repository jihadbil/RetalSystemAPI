using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Accounting.Pos;

/// <summary>
/// ناقل بيانات إنشاء نقطة بيع جديدة (POS Terminal Creation DTO).
/// </summary>
public class CreatePosTerminalDto
{
    /// <summary>كود نقطة البيع الفريد ضمن المنشأة</summary>
    [Required, MaxLength(50)] public string Code { get; set; } = null!;

    /// <summary>اسم أو وصف نقطة البيع (مثال: كاشير 1، نقطة بيع الصالة)</summary>
    [Required, MaxLength(150)] public string Name { get; set; } = null!;

    /// <summary>معرف الفرع التابعة له نقطة البيع</summary>
    [Required, NotEmptyGuid] public Guid BranchId { get; set; }

    /// <summary>معرف درج النقدية / الخزينة الافتراضية المرتبطة بهذه النقطة</summary>
    [Required, NotEmptyGuid] public Guid DefaultCashDrawerId { get; set; }
}

/// <summary>
/// ناقل بيانات تحديث نقطة بيع قائمة (POS Terminal Update DTO).
/// </summary>
public class UpdatePosTerminalDto : CreatePosTerminalDto
{
    /// <summary>حالة تفعيل نقطة البيع للعمل واستقبال المعاملات</summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// ناقل بيانات استجابة تفاصيل نقطة البيع (POS Terminal Response DTO).
/// </summary>
public class PosTerminalResponseDto
{
    /// <summary>المعرف الفريد لنقطة البيع</summary>
    public Guid Id { get; set; }

    /// <summary>كود نقطة البيع</summary>
    public string Code { get; set; } = null!;

    /// <summary>اسم نقطة البيع</summary>
    public string Name { get; set; } = null!;

    /// <summary>معرف الفرع التابعة له</summary>
    public Guid BranchId { get; set; }

    /// <summary>اسم الفرع</summary>
    public string BranchName { get; set; } = null!;

    /// <summary>معرف درج النقدية الافتراضي</summary>
    public Guid DefaultCashDrawerId { get; set; }

    /// <summary>اسم درج النقدية الافتراضي</summary>
    public string DefaultCashDrawerName { get; set; } = null!;

    /// <summary>حالة نشاط النقطة</summary>
    public bool IsActive { get; set; }

    /// <summary>تاريخ وتوقيت آخر اتصال أو نشاط مسجل للجهاز</summary>
    public DateTime? LastSeenAt { get; set; }
}

/// <summary>
/// ناقل بيانات استجابة كود الاقتران السريع لربط جهاز بنقطة بيع.
/// </summary>
public class PairingCodeResponseDto
{
    /// <summary>معرف المستأجر / المؤسسة</summary>
    public Guid TenantId { get; set; }

    /// <summary>معرف نقطة البيع المراد إقرانها</summary>
    public Guid TerminalId { get; set; }

    /// <summary>كود الاقتران المؤقت المكون من حروف أو أرقام</summary>
    public string PairingCode { get; set; } = null!;

    /// <summary>تاريخ ووقت انتهاء صلاحية كود الاقتران</summary>
    public DateTime ExpiresAt { get; set; }
}

/// <summary>
/// ناقل بيانات تسجيل وربط جهاز فيزيائي بنقطة البيع عبر كود الاقتران.
/// </summary>
public class ClaimPosDeviceDto
{
    /// <summary>معرف المستأجر</summary>
    [Required, NotEmptyGuid] public Guid TenantId { get; set; }

    /// <summary>كود الاقتران الصادر مسبقاً</summary>
    [Required, MaxLength(20)] public string PairingCode { get; set; } = null!;
}

/// <summary>
/// ناقل بيانات استجابة إتمام اقتران الجهاز بنقطة البيع مع رمز الوصول الخاص به.
/// </summary>
public class ClaimPosDeviceResponseDto
{
    /// <summary>رمز مميز طويل الأمد لتوثيق هوية الجهاز فيزيائياً (Device Token)</summary>
    public string DeviceToken { get; set; } = null!;

    /// <summary>تاريخ انتهاء صلاحية الرمز المميز للجهاز</summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>بيانات نقطة البيع المقترنة</summary>
    public PosTerminalResponseDto Terminal { get; set; } = null!;
}

/// <summary>
/// ناقل بيانات بدء جلسة كاشير جديدة وفتح الوردية.
/// </summary>
public class OpenCashSessionDto
{
    /// <summary>الرصيد الافتتاحي النقدي لدرج الكاشير (عهدة البداية / الفكة)</summary>
    [Range(0, double.MaxValue), MoneyAmount] public decimal OpeningFloat { get; set; }
}

/// <summary>
/// ناقل بيانات إغلاق جلسة الكاشير مع تسجيل الجرد الفعلي.
/// </summary>
public class CloseCashSessionDto
{
    /// <summary>الرصيد النقدي الفعلي المحصي والمعدود داخل الدرج عند الإغلاق</summary>
    [Range(0, double.MaxValue), MoneyAmount] public decimal CountedClosingBalance { get; set; }

    /// <summary>ملاحظات الكاشير أو مبررات الفروقات النقدية إن وجدت</summary>
    [MaxLength(500)] public string? Notes { get; set; }
}

/// <summary>
/// ناقل بيانات اعتماد وموافقة المشرف على فروقات جلسة النقدية (العجز أو الزيادة).
/// </summary>
public class ApproveCashDifferenceDto
{
    /// <summary>ملاحظات المشرف أو سبب التسوية المالية للفارق</summary>
    [Required, MaxLength(500)] public string Notes { get; set; } = null!;
}

/// <summary>
/// ناقل بيانات استجابة تفاصيل جلسة الكاشير / الوردية المالية.
/// </summary>
public class CashSessionResponseDto
{
    /// <summary>المعرف الفريد لجلسة الكاشير</summary>
    public Guid Id { get; set; }

    /// <summary>معرف الفرع</summary>
    public Guid BranchId { get; set; }

    /// <summary>معرف نقطة البيع</summary>
    public Guid PosTerminalId { get; set; }

    /// <summary>اسم نقطة البيع</summary>
    public string TerminalName { get; set; } = null!;

    /// <summary>معرف درج النقدية / الخزينة</summary>
    public Guid CashDrawerId { get; set; }

    /// <summary>اسم درج النقدية</summary>
    public string CashDrawerName { get; set; } = null!;

    /// <summary>معرف المستخدم الكاشير صاحب الوردية</summary>
    public string CashierUserId { get; set; } = null!;

    /// <summary>تاريخ ووقت فتح الوردية</summary>
    public DateTime OpenedAt { get; set; }

    /// <summary>الرصيد النقدي الافتتاحي (العهدة)</summary>
    public decimal OpeningFloat { get; set; }

    /// <summary>الرصيد الدفتري المتوقع في نهاية الوردية (الافتتاحي + المقبوضات - المدفوعات)</summary>
    public decimal ExpectedClosingBalance { get; set; }

    /// <summary>الرصيد الفعلي المعدود عند الإغلاق</summary>
    public decimal? CountedClosingBalance { get; set; }

    /// <summary>الفارق المالي بين الفعلي والدفتري (موجب = زيادة، سالب = عجز)</summary>
    public decimal? Difference { get; set; }

    /// <summary>تاريخ ووقت إغلاق الوردية</summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>حالة جلسة النقدية الحالية (مفتوحة، مغلقة، بانتظار الاعتماد)</summary>
    public CashSessionStatus Status { get; set; }

    /// <summary>ملاحظات الكاشير المسجلة عند الإغلاق</summary>
    public string? ClosingNotes { get; set; }

    /// <summary>معرف المشرف الذي اعتمد الفروقات</summary>
    public string? ApprovedByUserId { get; set; }

    /// <summary>تاريخ ووقت اعتماد المشرف</summary>
    public DateTime? ApprovedAt { get; set; }
}

/// <summary>
/// ناقل بيانات سياق نقطة البيع الفوري (الجهاز والجلسة النشطة الحالية).
/// </summary>
public class PosContextDto
{
    /// <summary>بيانات نقطة البيع الحالية</summary>
    public PosTerminalResponseDto Terminal { get; set; } = null!;

    /// <summary>بيانات الوردية المفتوحة حالياً على الجهاز (إن وجدت)</summary>
    public CashSessionResponseDto? CurrentSession { get; set; }
}
