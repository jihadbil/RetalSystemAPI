using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Branchs;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.Accounting;

/// <summary>
/// يمثل الخزائن المالية والحسابات النقدية والمصرفية في النظام (بنوك، صناديق نقدية عامة، أو أدراج نقود الكاشير).
/// </summary>
// كلاس كيان الخزينة المالية والحساب المصرفي
public class Safe : TenantBaseEntity
{
    /// <summary>
    /// اسم الخزينة أو الحساب المصرفي أو مسمى درج الكاشير.
    /// </summary>
    // خاصية اسم الخزينة
    public required string Name { get; set; }

    /// <summary>
    /// نوع الخزينة (حساب بنكي، صندوق نقدي، درج نقود كاشير).
    /// </summary>
    // خاصية نوع الخزينة من تعداد SafeType
    public SafeType Type { get; set; } = SafeType.CashBox;

    /// <summary>
    /// معرّف الفرع التابعة له الخزينة أو الحساب.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الفرع
    public Guid BranchId { get; set; }

    /// <summary>
    /// كائن الفرع التابعة له الخزينة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الفرع
    public Branch Branch { get; set; } = null!;

    /// <summary>
    /// الرصيد الافتتاحي المقيد للخزينة عند إنشائها في النظام.
    /// </summary>
    // خاصية الرصيد الافتتاحي للخزينة
    public decimal OpeningBalance { get; set; } = 0;

    /// <summary>
    /// الرصيد الفعلي الحالي للخزينة (يُحدَّث آلياً مع كل حركة واردة أو منصرفة).
    /// </summary>
    // خاصية الرصيد الدفتري الحالي للخزينة
    public decimal CurrentBalance { get; set; } = 0;

    /// <summary>
    /// رقم الحساب المصرفي أو الآيبان (مخصص للحسابات البنكية فقط).
    /// </summary>
    // خاصية رقم الحساب البنكي
    public string? BankAccountNumber { get; set; }

    /// <summary>
    /// اسم المصرف أو المؤسسة المالية المودع بها الحساب.
    /// </summary>
    // خاصية اسم البنك أو المصرف
    public string? BankName { get; set; }

    /// <summary>
    /// حالة نشاط الخزينة وما إذا كانت متاحة لإجراء الحركات والمعاملات.
    /// </summary>
    // خاصية مؤشر نشاط الخزينة
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// ملاحظات وبيانات إيضاحية إضافية حول الخزينة.
    /// </summary>
    // خاصية الملاحظات العامة للخزينة
    public string? Notes { get; set; }

    /// <summary>
    /// سجل المعاملات والحركات المالية الواردة والصادرة على هذه الخزينة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة حركات الخزينة
    public ICollection<SafeTransaction> Transactions { get; set; } = new List<SafeTransaction>();

    /// <summary>
    /// قائمة طرفيات نقاط البيع التي تعتمد هذه الخزينة كدرج افتراضي (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لطرفيات نقاط البيع المرتبطة
    public ICollection<PosTerminal> PosTerminals { get; set; } = new List<PosTerminal>();

    /// <summary>
    /// قائمة جلسات وورديات الكاشير التي تم ربطها بهذا الدرج النقدي (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لجلسات الكاشير المرتبطة
    public ICollection<CashSession> CashSessions { get; set; } = new List<CashSession>();
}
