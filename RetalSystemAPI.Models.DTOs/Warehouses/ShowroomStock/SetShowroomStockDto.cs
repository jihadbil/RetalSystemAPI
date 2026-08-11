using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Warehouses.ShowroomStock;

public class SetShowroomStockDto
{
    [Required(ErrorMessage = "معرف الصالة مطلوب")]
    public Guid WarehouseId { get; set; }

    [Required(ErrorMessage = "معرف المنتج مطلوب")]
    public Guid ProductId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "الكمية لا يمكن أن تكون سالبة")]
    public decimal Quantity { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "الحد الأدنى للمخزون لا يمكن أن يكون سالباً")]
    public int MinStockLevel { get; set; }
}
