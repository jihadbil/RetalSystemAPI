using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Sales;

/// <summary>
/// يمثل بنداً واحداً من بنود مرتجع المبيعات.
/// </summary>
public class SalesReturnItem : TenantBaseEntity
{
    public Guid SalesReturnId { get; set; }
    public SalesReturn SalesReturn { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid? ProductBarCodeId { get; set; }
    public ProductBarCode? ProductBarCode { get; set; }

    /// <summary>
    /// الكمية المرتجعة.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// السعر المعتمد للإرجاع للوحدة.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// إجمالي قيمة البند المرتجع.
    /// </summary>
    public decimal LineTotal { get; set; }

    public string? Notes { get; set; }
}
