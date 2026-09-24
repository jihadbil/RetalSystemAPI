using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Accounting;

/// <summary>
/// يمثل طرفية أو جهاز نقطة البيع (POS Terminal) المعرّف في الفرع لتسجيل المبيعات وجلسات الصندوق.
/// </summary>
// كلاس كيان طرفية نقطة البيع
public class PosTerminal : TenantBaseEntity
{
    /// <summary>
    /// الرمز أو الكود التعريفي الفريد للطرفية (مثال: POS-01).
    /// </summary>
    // خاصية كود الطرفية الفريد
    public required string Code { get; set; }

    /// <summary>
    /// الاسم الوصفي لطرفية نقطة البيع (مثال: كاشير 1 - الصالة الرئيسية).
    /// </summary>
    // خاصية اسم نقطة البيع
    public required string Name { get; set; }

    /// <summary>
    /// معرّف الفرع التابعة له طرفية نقطة البيع.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الفرع
    public Guid BranchId { get; set; }

    /// <summary>
    /// كائن الفرع التابعة له الطرفية (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الفرع
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// معرّف درج النقدية الافتراضي المرتبط بهذه الطرفية.
    /// </summary>
    // خاصية المفتاح الأجنبي لدرج النقدية الافتراضي
    public Guid DefaultCashDrawerId { get; set; }

    /// <summary>
    /// كائن درج النقدية الافتراضي المرتبط بالطرفية (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان درج النقدية الافتراضي
    public Safe DefaultCashDrawer { get; set; } = null!;

    /// <summary>
    /// حالة نشاط الطرفية وما إذا كانت مفعلة لاستقبال العمليات.
    /// </summary>
    // خاصية مؤشر حالة نشاط الطرفية
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// تاريخ وتوقيت آخر اتصال أو نشاط مسجل للطرفية مع النظام.
    /// </summary>
    // خاصية تاريخ ووقت آخر ظهور للطرفية
    public DateTime? LastSeenAt { get; set; }

    /// <summary>
    /// تجزئة رمز الاقتران (Pairing Code Hash) لربط وتوثيق الجهاز الفيزيائي الجديد لأول مرة.
    /// </summary>
    // خاصية تجزئة كود اقتران الجهاز
    public string? PairingCodeHash { get; set; }

    /// <summary>
    /// تاريخ وتوقيت انتهاء صلاحية كود الاقتران الأولي.
    /// </summary>
    // خاصية تاريخ ووقت انتهاء كود الاقتران
    public DateTime? PairingCodeExpiresAt { get; set; }

    /// <summary>
    /// قائمة سجلات الاعتماد والتوثيق الأمني المرتبطة بالجهاز (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة اعتمادات الأجهزة
    public ICollection<PosDeviceCredential> DeviceCredentials { get; set; } = new List<PosDeviceCredential>();

    /// <summary>
    /// قائمة جلسات وورديات الكاشير التي تم فتحها وتشغيلها عبر هذه الطرفية (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة جلسات الكاشير
    public ICollection<CashSession> CashSessions { get; set; } = new List<CashSession>();
}
