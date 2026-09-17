using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;

/// <summary>
/// ناقل بيانات بند التسوية الجردية (Stock Adjustment Item DTO).
/// يتضمن الرصيد الدفتري والفعلي وتكلفة الوحدة وسبب تسوية البند.
/// </summary>
public class StockAdjustmentItemDto
{
    /// <summary>المعرف الفريد للمنتج</summary>
    [Required(ErrorMessage = "معرف الصنف مطلوب")]
    public Guid ProductId { get; set; }

    /// <summary>معرف الباركود إن وجد</summary>
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>الكمية الدفترية المسجلة في النظام قبل الجرد</summary>
    public int SystemQuantity { get; set; }

    /// <summary>الكمية الفعلية المحصورة أثناء الجرد</summary>
    [Range(0, int.MaxValue, ErrorMessage = "الكمية الفعلية يجب أن تكون أكبر من أو تساوي 0")]
    public int ActualQuantity { get; set; }

    /// <summary>تكلفة الوحدة لتقييم الفارق المالي</summary>
    [Range(0, double.MaxValue, ErrorMessage = "تكلفة الوحدة يجب أن تكون أكبر من أو تساوي 0")]
    public decimal UnitCost { get; set; }

    /// <summary>سبب تسوية هذا البند تحديداً؛ إن لم يُرسل يُعتمد سبب التسوية العام</summary>
    public StockAdjustmentReason? Reason { get; set; }
}
