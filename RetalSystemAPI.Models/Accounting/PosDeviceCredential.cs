using System;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Accounting;

/// <summary>
/// يمثل بيانات اعتماد وتوثيق جهاز طرفية نقطة البيع (POS Device Credentials) والرموز الأمنية المخصصة للاتصال بالخادم.
/// </summary>
// كلاس كيان بيانات اعتماد وتوثيق أجهزة نقاط البيع
public class PosDeviceCredential : TenantBaseEntity
{
    /// <summary>
    /// معرّف طرفية نقطة البيع المرتبطة بهذه الاعتمادية.
    /// </summary>
    // خاصية المفتاح الأجنبي لطرفية نقطة البيع
    public Guid PosTerminalId { get; set; }

    /// <summary>
    /// كائن طرفية نقطة البيع المرتبطة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان طرفية نقطة البيع
    public PosTerminal PosTerminal { get; set; } = null!;

    /// <summary>
    /// تجزئة التوكن المشفرة (Token Hash) المستخدمة للتحقق من هوية الجهاز دون تخزين الرمز الصريح.
    /// </summary>
    // خاصية تجزئة رمز المصادقة المشفر للجهاز
    public required string TokenHash { get; set; }

    /// <summary>
    /// تاريخ وتوقيت انتهاء صلاحية رمز الاعتماد الأمني.
    /// </summary>
    // خاصية تاريخ وتوقيت انتهاء صلاحية التوكن
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// تاريخ وتوقيت آخر اتصال أو نشاط مسجل للجهاز مع الخادم.
    /// </summary>
    // خاصية تاريخ ووقت آخر ظهور أو اتصال للجهاز
    public DateTime? LastSeenAt { get; set; }

    /// <summary>
    /// تاريخ وتوقيت إلغاء أو إبطال صلاحية الاعتماد الأمني في حال إيقاف الجهاز.
    /// </summary>
    // خاصية تاريخ وتوقيت إلغاء الصلاحية
    public DateTime? RevokedAt { get; set; }
}
