using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;

/// <summary>
/// ناقل بيانات إنشاء تسوية جردية للمخزون (Create Stock Adjustment DTO).
/// يُستخدم لإثبات فروقات الجرد الفعلي عن الرصيد الدفتري (عجز أو زيادة).
/// </summary>
public class CreateStockAdjustmentDto
{
    /// <summary>رقم إشعار التسوية الجردية</summary>
    [Required(ErrorMessage = "رقم التسوية الجردية مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم التسوية الجردية يجب أن لا يتجاوز 50 حرف")]
    public string AdjustmentNumber { get; set; } = null!;

    /// <summary>تاريخ وتوقيت إجراء الجرد والتسوية</summary>
    public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;

    /// <summary>معرف المستودع أو الصالة الخاضعة للتسوية</summary>
    [Required(ErrorMessage = "معرف المستودع أو الصالة مطلوب")]
    public Guid WarehouseId { get; set; }

    /// <summary>السبب العام للتسوية الجردية (جرد دوري، بضاعة تالفة، منتهية، رصيد افتتاحي)</summary>
    public StockAdjustmentReason Reason { get; set; }

    /// <summary>ملاحظات إضافية حول عملية الجرد</summary>
    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    /// <summary>قائمة بنود الأصناف الخاضعة للتسوية</summary>
    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل للتسوية")]
    public List<StockAdjustmentItemDto> Items { get; set; } = new();
}
