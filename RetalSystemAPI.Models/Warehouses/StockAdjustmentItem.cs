using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Warehouses;

/// <summary>
/// بند واحد من بنود التسوية الجردية.
/// </summary>
public class StockAdjustmentItem : TenantBaseEntity
{
    public Guid StockAdjustmentId { get; set; }
    public StockAdjustment StockAdjustment { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid? ProductBarCodeId { get; set; }
    public ProductBarCode? ProductBarCode { get; set; }

    /// <summary>
    /// الكمية المسجلة في النظام قبل التسوية.
    /// </summary>
    public int SystemQuantity { get; set; }

    /// <summary>
    /// الكمية الفعلية بعد الجرد.
    /// </summary>
    public int ActualQuantity { get; set; }

    /// <summary>
    /// الفارق (الفعلي - المسجل): موجب يعني زيادة، سالب يعني عجز.
    /// </summary>
    public int DifferenceQuantity { get; set; }

    /// <summary>
    /// تكلفة الوحدة لحساب الأثر المالي للتسوية.
    /// </summary>
    public decimal UnitCost { get; set; }
}
