using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;

/// <summary>
/// ناقل بيانات تعيين أو تعديل رصيد صالة العرض لصنف معين (Set Showroom Stock DTO).
/// </summary>
public class SetShowroomStockDto
{
    /// <summary>المعرف الفريد لمستودع صالة العرض</summary>
    [Required(ErrorMessage = "معرف الصالة مطلوب")]
    public Guid WarehouseId { get; set; }

    /// <summary>المعرف الفريد للمنتج</summary>
    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid ProductId { get; set; }

    /// <summary>الكمية الفعلية المعروضة بالصالة</summary>
    [Range(0, double.MaxValue, ErrorMessage = "الكمية لا يمكن أن تكون سالبة")]
    public decimal Quantity { get; set; }

    /// <summary>حد الطلب الأدنى لرصيد الصالة للتنبيه عند النقص</summary>
    [Range(0, int.MaxValue, ErrorMessage = "الحد الأدنى للمخزون لا يمكن أن يكون سالباً")]
    public int MinStockLevel { get; set; }
}
