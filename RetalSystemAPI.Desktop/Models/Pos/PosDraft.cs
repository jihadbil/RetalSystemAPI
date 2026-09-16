using System.Text.Json;
using RetalSystemAPI.Desktop.Models.Sales;

namespace RetalSystemAPI.Desktop.Models.Pos;

public sealed class PosDraft
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateTime SavedAt { get; set; } = DateTime.Now;
    public Guid? BranchId { get; set; }
    public Guid? WarehouseId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public List<CartItemModel> Items { get; set; } = new();
    public decimal Discount { get; set; }
    public decimal Tax { get; set; }
    public decimal Paid { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public string Notes { get; set; } = string.Empty;
    public decimal Total => Math.Max(0, Items.Sum(i => i.LineTotal) - Discount + Tax);
    public string Label => $"{SavedAt:HH:mm} · {CustomerName ?? "عميل نقدي"} · {Items.Count} أصناف · {Total:N2} د.ل";
}

public sealed class PosSessionState
{
    public PosDraft? Active { get; set; }
    public List<PosDraft> Held { get; set; } = new();

    // Detach saved items from the live observable collection and its event handlers.
    public PosSessionState Copy() => JsonSerializer.Deserialize<PosSessionState>(JsonSerializer.Serialize(this))!;
}
