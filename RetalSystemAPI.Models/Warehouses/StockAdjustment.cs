using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Enums;

namespace RetalSystemAPI.Models.Warehouses;

/// <summary>
/// يمثل تسوية جردية لمعالجة الفوارق بين الكمية المسجلة في النظام والكمية الفعلية.
/// </summary>
public class StockAdjustment : TenantBaseEntity
{
    /// <summary>
    /// رقم التسوية الجردية.
    /// </summary>
    public required string AdjustmentNumber { get; set; }

    /// <summary>
    /// تاريخ التسوية.
    /// </summary>
    public DateTime AdjustmentDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// المستودع أو الصالة التي تمت فيها التسوية.
    /// </summary>
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// سبب التسوية (جرد، تلف، انتهاء صلاحية، إلخ).
    /// </summary>
    public StockAdjustmentReason Reason { get; set; }

    public string? Notes { get; set; }

    public ICollection<StockAdjustmentItem> Items { get; set; } = new List<StockAdjustmentItem>();
}
