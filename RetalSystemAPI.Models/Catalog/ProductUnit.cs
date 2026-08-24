using System;
using System.Collections.Generic;
using System.Text;
using RetalSystemAPI.Models.Common;
namespace RetalSystemAPI.Models.Catalog;
/// <summary>
/// يمتل وحدات المنتجات حيث ان المنتج يمكت ان يصل على هيئة صندوق او دستة او قطعة و هذا يساعد على ادخال البضاعة و حساب تكلفة القطعة بدل تغيير العبوة في كل عملية حساب
/// </summary>
public class ProductUnit : TenantBaseEntity
{

    /// <summary>
    /// معرف المنتج الدي تنتمي اليه الوحدة
    /// </summary>
    public Guid ProductId { get; set; }
    public Product? Product { get; set; }
    /// <summary>
    /// معرف الوحدة
    /// </summary>
    public Guid UnitId { get; set; }
    public Unit? Unit { get; set; }

    public int ConversionFactor { get; set; }

    public bool IsDefault { get; set; } = false;    
}
