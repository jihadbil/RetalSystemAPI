using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

public class SalesInvoiceResponseDto : BaseDto
{
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public Guid BranchId { get; set; }
    public string BranchName { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = null!;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = null!;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public string? Notes { get; set; }
    public List<SalesInvoiceItemResponseDto> Items { get; set; } = new();
}
