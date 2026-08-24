using System.ComponentModel.DataAnnotations;

namespace RetalSystemAPI.Models.DTOs.Warehouses.StockTransfer;

public class StockTransferItemDto
{
    [Required(ErrorMessage = "معرف الصنف مطلوب")]
    public Guid ProductId { get; set; }

    public Guid? ProductBarCodeId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "الكمية المحولة يجب أن تكون 1 على الأقل")]
    public int Quantity { get; set; }

    [MaxLength(200, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 200 حرف")]
    public string? Notes { get; set; }
}
