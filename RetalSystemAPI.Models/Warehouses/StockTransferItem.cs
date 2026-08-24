using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Warehouses;

/// <summary>
/// بند واحد من بنود أمر التحويل المخزني.
/// </summary>
public class StockTransferItem : TenantBaseEntity
{
    public Guid StockTransferId { get; set; }
    public StockTransfer StockTransfer { get; set; } = null!;

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    /// <summary>
    /// الباركود المحدد عند التحويل من/إلى مخزن رئيسي.
    /// </summary>
    public Guid? ProductBarCodeId { get; set; }
    public ProductBarCode? ProductBarCode { get; set; }

    /// <summary>
    /// الكمية المحولة.
    /// </summary>
    public int Quantity { get; set; }

    public string? Notes { get; set; }
}
