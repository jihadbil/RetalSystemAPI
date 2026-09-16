using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Purchase;

/// <summary>
/// يمثل بنداً واحداً من بنود مرتجع المشتريات.
/// </summary>
public class PurchaseReturnItem : TenantBaseEntity
{
    public Guid PurchaseReturnId { get; set; }
    public PurchaseReturn PurchaseReturn { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>
    /// الباركود / النكهة المحددة المرتجعة من المخزن.
    /// </summary>
    public Guid? ProductBarCodeId { get; set; }
    public ProductBarCode? ProductBarCode { get; set; }

    /// <summary>
    /// الكمية المرتجعة.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// سعر شراء الوحدة المعتمد في المرتجع.
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// إجمالي قيمة البند المرتجع (الكمية × سعر الوحدة).
    /// </summary>
    public decimal LineTotal { get; set; }

    /// <summary>
    /// ملاحظات على البند.
    /// </summary>
    public string? Notes { get; set; }
}
