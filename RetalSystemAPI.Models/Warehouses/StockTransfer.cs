using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.Warehouses;

/// <summary>
/// يمثل عملية نقل البضاعة بين مستودعين أو صالتين
/// (مثال: من المخزن الرئيسي إلى صالة العرض أو بين الفروع).
/// </summary>
public class StockTransfer : TenantBaseEntity
{
    /// <summary>
    /// رقم أمر التحويل.
    /// </summary>
    public required string TransferNumber { get; set; }

    /// <summary>
    /// تاريخ التحويل.
    /// </summary>
    public DateTime TransferDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// المستودع أو الصالة المصدر.
    /// </summary>
    public Guid FromWarehouseId { get; set; }
    public Warehouse FromWarehouse { get; set; } = null!;

    /// <summary>
    /// المستودع أو الصالة الوجهة.
    /// </summary>
    public Guid ToWarehouseId { get; set; }
    public Warehouse ToWarehouse { get; set; } = null!;

    public StockTransferStatus Status { get; set; } = StockTransferStatus.Draft;

    public string? Notes { get; set; }

    public ICollection<StockTransferItem> Items { get; set; } = new List<StockTransferItem>();
}
