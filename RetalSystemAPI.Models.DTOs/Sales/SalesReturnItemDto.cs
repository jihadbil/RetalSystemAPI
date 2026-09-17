using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Sales;

/// <summary>
/// ناقل بيانات بند مرتجع المبيعات (Sales Return Item DTO).
/// </summary>
public class SalesReturnItemDto
{
    /// <summary>معرف الصنف المرتجع</summary>
    [Required(ErrorMessage = "معرف الصنف مطلوب")]
    public Guid ProductId { get; set; }

    /// <summary>معرف الباركود إن وجد</summary>
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>الكمية المرتجعة</summary>
    [Range(1, int.MaxValue, ErrorMessage = "الكمية المرتجعة يجب أن تكون 1 على الأقل")]
    public int Quantity { get; set; }

    /// <summary>سعر الإرجاع للوحدة</summary>
    [Range(0, double.MaxValue, ErrorMessage = "سعر الإرجاع للوحدة يجب أن يكون أكبر من أو يساوي 0")]
    public decimal UnitPrice { get; set; }

    /// <summary>ملاحظات إضافية حول سبب الإرجاع للبند</summary>
    [MaxLength(200, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 200 حرف")]
    public string? Notes { get; set; }
}
