using System;
using System.Collections.Generic;
using System.Text;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Catalog;
/// <summary>
/// يمتل الباركود الخاص بالمنتج بحيث يمكن ان يكون للمنتج اكثر من باركود لدعم تعدد الأكواد للصنف الواحد
/// </summary>
public class ProductBarCode:BaseEntity
{
    /// <summary>
    /// الباركود الخاص بالمنتج
    /// </summary>
    public string BarCode { get; set; }
    /// <summary>
    /// اسم المنتج متلا عصير النسيم برتقال-عصير النسيم مانجا
    /// </summary>
    public string Title { get; set; }
    /// <summary>
    /// الوصف
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// معرف المنتج الدي ينتمي له الكود
    /// </summary>
    public Guid ProductId { get; set; }

    public Product? Product { get; set; }
    /// <summary>
    /// معر المستاجر
    /// </summary>
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }

    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();    
}
