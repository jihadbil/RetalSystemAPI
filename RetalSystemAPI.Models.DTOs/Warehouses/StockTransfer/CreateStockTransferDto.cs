using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;

public class CreateStockTransferDto
{
    [Required(ErrorMessage = "رقم أمر التحويل مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم أمر التحويل يجب أن لا يتجاوز 50 حرف")]
    public string TransferNumber { get; set; } = null!;

    public DateTime TransferDate { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "المستودع المصدر مطلوب")]
    public Guid FromWarehouseId { get; set; }

    [Required(ErrorMessage = "المستودع الوجهة مطلوب")]
    public Guid ToWarehouseId { get; set; }

    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل للتحويل")]
    public List<StockTransferItemDto> Items { get; set; } = new();
}
