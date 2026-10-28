using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Sales;

namespace RetalSystemAPI.Models.Accounting;

/// <summary>تحصيل بوسيلة محددة في سجل الوردية؛ غير النقد يبقى معلقاً حتى التسوية الاختيارية.</summary>
public class CashSessionMovement : TenantBaseEntity
{
    public Guid CashSessionId { get; set; }
    public CashSession CashSession { get; set; } = null!;
    public Guid SalesInvoiceId { get; set; }
    public SalesInvoice SalesInvoice { get; set; } = null!;
    public Guid ClientMovementId { get; set; }
    public FinancialDocumentType DocumentType { get; set; }
    public Guid DocumentId { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public TransactionDirection Direction { get; set; }
    public decimal Amount { get; set; }
    public decimal? TenderedAmount { get; set; }
    public decimal ReversedAmount { get; set; }
    public Guid? OriginalMovementId { get; set; }
    public CashSessionMovement? OriginalMovement { get; set; }
    public Guid? SettlementId { get; set; }
    public CashSessionPaymentSettlement? Settlement { get; set; }
    public Guid? SettlementSafeId { get; set; }
    public Safe? SettlementSafe { get; set; }
}
