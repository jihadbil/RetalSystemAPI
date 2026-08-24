using System.ComponentModel.DataAnnotations;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

public class CreateSalesReturnDto
{
    [Required(ErrorMessage = "رقم الإرجاع مطلوب")]
    [MaxLength(50, ErrorMessage = "رقم الإرجاع يجب أن لا يتجاوز 50 حرف")]
    public string ReturnNumber { get; set; } = null!;

    public DateTime ReturnDate { get; set; } = DateTime.UtcNow;

    public Guid? OriginalInvoiceId { get; set; }

    [Required(ErrorMessage = "معرف الفرع مطلوب")]
    public Guid BranchId { get; set; }

    [Required(ErrorMessage = "معرف المستودع أو الصالة مطلوب")]
    public Guid WarehouseId { get; set; }

    public Guid? CustomerId { get; set; }

    public SalesReturnReason Reason { get; set; } = SalesReturnReason.Other;

    [MaxLength(500, ErrorMessage = "الملاحظات يجب أن لا تتجاوز 500 حرف")]
    public string? Notes { get; set; }

    [MinLength(1, ErrorMessage = "يجب إضافة بند واحد على الأقل للمرتجع")]
    public List<SalesReturnItemDto> Items { get; set; } = new();
}
