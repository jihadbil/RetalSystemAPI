using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StorgeStock;

public class SetStorgeStockDto
{
    [Required(ErrorMessage = "معرف المخزن مطلوب")]
    public Guid WarehouseId { get; set; }

    [Required(ErrorMessage = "معرف الباركود مطلوب")]
    public Guid ProductBarcodeId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "الكمية لا يمكن أن تكون سالبة")]
    public decimal Quantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "الحد الأدنى للمخزون لا يمكن أن يكون سالباً")]
    public int MinStockLevel { get; set; }
}
