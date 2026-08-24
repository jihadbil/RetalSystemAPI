using RetalSystemAPI.Models.DTOs.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.DTOs.Sales;

public class SalesInvoiceSummaryDto : BaseDto
{
    public string InvoiceNumber { get; set; } = null!;
    public DateTime InvoiceDate { get; set; }
    public string BranchName { get; set; } = null!;
    public string WarehouseName { get; set; } = null!;
    public string? CustomerName { get; set; }
    public InvoiceStatus Status { get; set; }
    public string StatusName { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; }
    public string PaymentMethodName { get; set; } = null!;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public int ItemCount { get; set; }
}
