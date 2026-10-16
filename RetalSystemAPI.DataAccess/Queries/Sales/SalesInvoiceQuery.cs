using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Queries;
public sealed record SalesInvoiceQuery : PageRequest
{
    public Guid? BranchId { get; init; }
    public Guid? WarehouseId { get; init; }
    public Guid? CustomerId { get; init; }
    public InvoiceStatus? Status { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Search { get; init; }
}
