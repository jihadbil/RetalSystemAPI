using RetalSystemAPI.Models.DTOs.Common;
using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;

/// <summary>
/// ناقل بيانات تعيين أو تعديل رصيد المخزن الرئيسي لباركود محدد (Set Storage Stock DTO).
/// </summary>
public class SetStorgeStockDto
{
    /// <summary>المعرف الفريد للمخزن الرئيسي</summary>
    [Required(ErrorMessage = "معرف المخزن مطلوب")]
    [NotEmptyGuid]
    public Guid WarehouseId { get; set; }

    /// <summary>المعرف الفريد لباركود الصنف</summary>
    [Required(ErrorMessage = "معرف الباركود مطلوب")]
    [NotEmptyGuid]
    public Guid ProductBarcodeId { get; set; }

    /// <summary>الكمية الفعلية المتوفرة بالمخزن</summary>
    [Range(0, double.MaxValue, ErrorMessage = "الكمية لا يمكن أن تكون سالبة")]
    [WholeQuantity(AllowZero = true)]
    public decimal Quantity { get; set; }

    /// <summary>الحد الأدنى لمخزون المخزن (حد الطلب)</summary>
    [Range(0, int.MaxValue, ErrorMessage = "الحد الأدنى للمخزون لا يمكن أن يكون سالباً")]
    public int MinStockLevel { get; set; }
}
