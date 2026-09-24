using System;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.Accounting;

/// <summary>
/// يمثل جلسة أو وردية الكاشير (درج النقدية) في نقطة البيع، متضمناً الرصيد الافتتاحي، المتوقع، الفعلي، ومطابقة الفروقات.
/// </summary>
// كلاس كيان جلسة الكاشير لنقاط البيع
public class CashSession : TenantBaseEntity
{
    /// <summary>
    /// معرّف الفرع التابع له نقطة البيع وجلسة الكاشير.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الفرع
    public Guid BranchId { get; set; }

    /// <summary>
    /// كائن الفرع المرتبط بالجلسة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الفرع
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// معرّف جهاز أو طرفية نقطة البيع (POS Terminal) المستخدمة.
    /// </summary>
    // خاصية المفتاح الأجنبي لطرفية نقطة البيع
    public Guid PosTerminalId { get; set; }

    /// <summary>
    /// كائن طرفية نقطة البيع المرتبطة بالجلسة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان طرفية نقطة البيع
    public PosTerminal PosTerminal { get; set; } = null!;

    /// <summary>
    /// معرّف درج النقدية أو الخزينة المخصصة لهذه الجلسة.
    /// </summary>
    // خاصية المفتاح الأجنبي لدرج النقدية
    public Guid CashDrawerId { get; set; }

    /// <summary>
    /// كائن درج النقدية أو الخزينة المرتبطة بالجلسة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان درج النقدية
    public Safe CashDrawer { get; set; } = null!;

    /// <summary>
    /// معرّف المستخدم (الكاشير) المسؤول عن فتح وإدارة هذه الجلسة.
    /// </summary>
    // خاصية معرف مستخدم الكاشير
    public required string CashierUserId { get; set; }

    /// <summary>
    /// تاريخ وتوقيت افتتاح الجلسة والبدء في تسجيل المعاملات.
    /// </summary>
    // خاصية تاريخ وتوقيت افتتاح الجلسة
    public DateTime OpenedAt { get; set; }

    /// <summary>
    /// الرصيد الافتتاحي النقدي (العهدة النقدية الأولية) الموجودة في الدرج عند الفتح.
    /// </summary>
    // خاصية الرصيد الافتتاحي لدرج النقدية
    public decimal OpeningFloat { get; set; }

    /// <summary>
    /// الرصيد الإجمالي المتوقع في الدرج عند الإغلاق بناءً على المبيعات والمتحصلات المسجلة بالنظام.
    /// </summary>
    // خاصية الرصيد الختامي المتوقع دفترياً
    public decimal ExpectedClosingBalance { get; set; }

    /// <summary>
    /// الرصيد الفعلي المعدود في الدرج الذي قام الكاشير بحصره عند الإغلاق.
    /// </summary>
    // خاصية الرصيد الفعلي بعد الجرد
    public decimal? CountedClosingBalance { get; set; }

    /// <summary>
    /// الفرق بين الرصيد الفعلي والمتوقع (موجب يمثل زيادة، سالب يمثل عجزاً).
    /// </summary>
    // خاصية فرق الجرد النقدي
    public decimal? Difference { get; set; }

    /// <summary>
    /// تاريخ وتوقيت إغلاق الجلسة وإنهاء عمليات البيع فيها.
    /// </summary>
    // خاصية تاريخ وتوقيت إغلاق الجلسة
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// حالة الجلسة الحالية (مفتوحة، بانتظار الاعتماد، مغلقة، ملغاة).
    /// </summary>
    // خاصية حالة جلسة الكاشير
    public CashSessionStatus Status { get; set; } = CashSessionStatus.Open;

    /// <summary>
    /// ملاحظات وتبريرات الإغلاق أو أسباب الفروقات النقدية المدونة من الكاشير.
    /// </summary>
    // خاصية ملاحظات وتفاصيل إغلاق الجلسة
    public string? ClosingNotes { get; set; }

    /// <summary>
    /// معرّف المشرف أو المدير الذي قام بمراجعة واعتماد الجلسة.
    /// </summary>
    // خاصية معرف المستخدم المشرف المعتمد للجلسة
    public string? ApprovedByUserId { get; set; }

    /// <summary>
    /// تاريخ وتوقيت اعتماد إغلاق الجلسة وترحيل فروقاتها.
    /// </summary>
    // خاصية تاريخ وتوقيت الاعتماد
    public DateTime? ApprovedAt { get; set; }
}
