using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Queries;
public sealed record ExpenseQuery : PageRequest
{
    public Guid? BranchId { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? SafeId { get; init; }
    public PaymentMethod? PaymentMethod { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? SearchTerm { get; init; }
}
