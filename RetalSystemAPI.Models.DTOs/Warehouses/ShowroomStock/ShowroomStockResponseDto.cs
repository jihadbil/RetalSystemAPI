using RetalSystemAPI.Models.DTOs.Common;

namespace RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;

/// <summary>
/// ناقل بيانات استجابة رصيد صالة العرض (Showroom Stock Response DTO).
/// يتضمن اسم الصالة والصنف والصورة والكمية المتوفرة وتنبيه النقص.
/// </summary>
public class ShowroomStockResponseDto : BaseDto
{
    /// <summary>معرف صالة العرض</summary>
    public Guid WarehouseId { get; set; }

    /// <summary>اسم صالة العرض</summary>
    public string WarehouseName { get; set; } = null!;

    /// <summary>معرف المنتج</summary>
    public Guid ProductId { get; set; }

    /// <summary>اسم المنتج</summary>
    public string ProductName { get; set; } = null!;

    /// <summary>رابط صورة المنتج</summary>
    public string? ImageUrl { get; set; }

    /// <summary>الكمية الحالية المتوفرة بصالة العرض</summary>
    public decimal Quantity { get; set; }

    /// <summary>حد الطلب الأدنى المعتمد للصالة</summary>
    public int MinStockLevel { get; set; }

    /// <summary>هل الرصيد الحالي هبط دون الحد الأدنى</summary>
    public bool IsBelowMinLevel { get; set; }
}
