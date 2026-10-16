using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.DataAccess.Queries;
public sealed record SafeTransactionQuery : PageRequest
{
    public Guid? SafeId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public TransactionDirection? Direction { get; init; }
    public FinancialDocumentType? DocumentType { get; init; }
}
