using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Accounting;

public class CashSessionPaymentSettlement : TenantBaseEntity
{
    public Guid CashSessionId { get; set; }
    public CashSession CashSession { get; set; } = null!;
    public required string RequestHash { get; set; }
    public string? Notes { get; set; }
    public DateTime SettledAt { get; set; } = DateTime.UtcNow;
}
