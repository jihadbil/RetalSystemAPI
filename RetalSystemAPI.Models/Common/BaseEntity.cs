namespace RetalSystemAPI.Models.Common;
/// <summary>
/// النمودج الأساسي لجميع النماذج في النظام، يحتوي على الخصائص المشتركة بين جميع النماذج مثل معرف الكيان، معرف المستأجر، تاريخ الإنشاء، تاريخ التحديث، وحالة الحذف.
/// </summary>
public abstract class BaseEntity 
{
    /// <summary>
    /// معرف الكيان الفريد في قاعدة البيانات.
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// تاريخ إنشاء الكيان.
    /// </summary>
    public DateTime CreatedAt { get; set; }


    /// <summary>
    /// تاريخ آخر تحديث للكيان.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }


    /// <summary>
    /// حالة الحذف للكيان، حيث يشير إلى ما إذا كان الكيان قد تم حذفه من النظام أم لا.
    /// </summary>
    public bool IsDeleted { get; set; } = false;
    



    public string? CreatedByUserId { get; set; }
    public string? UpdatedByUserId { get; set; }
    public byte[] RowVersion { get; set; } = null!;
}
