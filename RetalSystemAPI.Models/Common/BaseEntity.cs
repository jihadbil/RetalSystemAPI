namespace RetalSystemAPI.Models.Common;

/// <summary>
/// الكيان الأساسي التجريدي لكافة جداول ونماذج النظام، يحتوي على معرّف الكيان، طوابع التدقيق الزمني، الحذف المنطقي، ومؤشر التزامن المتفائل.
/// </summary>
// الكلاس الأساسي المشترك لكافة الكيانات والنماذج في قاعدة البيانات
public abstract class BaseEntity 
{
    /// <summary>
    /// المعرّف الفريد للكيان (المفتاح الأساسي Primary Key).
    /// </summary>
    // خاصية المفتاح الأساسي للكيان
    public Guid Id { get; set; }

    /// <summary>
    /// تاريخ ووقت إنشاء السجل في النظام بالتوقيت العالمي UTC.
    /// </summary>
    // خاصية تاريخ ووقت الإنشاء
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// تاريخ ووقت آخر تعديل طرأ على السجل في النظام بالتوقيت العالمي UTC.
    /// </summary>
    // خاصية تاريخ ووقت آخر تحديث
    public DateTime? UpdatedAt { get; set; }

    /// <summary>
    /// حالة الحذف المنطقي (Soft Delete)، حيث تكون true إذا تم حذف السجل واستبعاده من الاستعلامات.
    /// </summary>
    // خاصية مؤشر الحذف المنطقي
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// معرّف المستخدم الذي قام بإنشاء هذا السجل لأغراض التدقيق والمتابعة (Audit Log).
    /// </summary>
    // خاصية معرف المستخدم المنشئ للسجل
    public string? CreatedByUserId { get; set; }

    /// <summary>
    /// معرّف المستخدم الذي قام بآخر تعديل على هذا السجل لأغراض التدقيق والمتابعة (Audit Log).
    /// </summary>
    // خاصية معرف المستخدم المعدل للسجل
    public string? UpdatedByUserId { get; set; }

    /// <summary>
    /// مصفوفة البايتات المستخدمة للتحكم في التزامن المتفائل (Optimistic Concurrency Token) لمنع تضارب التعديل المتزامن.
    /// </summary>
    // خاصية طابع إصدار الصف لمنع تضارب التعديلات
    public byte[] RowVersion { get; set; } = null!;
}
