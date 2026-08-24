using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockAdjustment;

public class CreateStockAdjustmentDto
{
    [Required(ErrorMessage = "رقم التسوية الجردية مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم التسوية الجردية يجب أن لا يتجاوز 50 حرف")]
    public string AdjustmentNumber { get; set; } = null!;

    public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "معرف المستودع أو الصالة مطلوب")]
    public Guid WarehouseId { get; set; }

    public StockAdjustmentReason Reason { get; set; }

    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل للتسوية")]
    public List<StockAdjustmentItemDto> Items { get; set; } = new();
}
