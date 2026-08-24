using System;
using System.Collections.Generic;
using System.Text;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.Models.Catalog;
/// <summary>
/// يمتل الباركود الخاص بالمنتج بحيث يمكن ان يكون للمنتج اكثر من باركود لدعم تعدد الأكواد للصنف الواحد
/// </summary>
public class ProductBarCode : TenantBaseEntity
{
    /// <summary>
    /// الباركود الخاص بالمنتج
    /// </summary>
    public required string BarCode { get; set; }
    /// <summary>
    /// اسم المنتج متلا عصير النسيم برتقال-عصير النسيم مانجا
    /// </summary>
    public required string Title { get; set; }
    /// <summary>
    /// الوصف
    /// </summary>
    public string? Description { get; set; }
    /// <summary>
    /// معرف المنتج الدي ينتمي له الكود
    /// </summary>
    public Guid ProductId { get; set; }

    public Product? Product { get; set; }

    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();    
    public ICollection<StorgeStock> StorgeStocks { get; set; } = new List<StorgeStock>();

  

    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
}
