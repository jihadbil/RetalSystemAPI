using System;
using System.Collections.Generic;
using System.Text;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Catalog;
/// <summary>
/// جدول بيانات ا لأصناف الأساسية في النظام (المنتجات) ويمتل كل صنف علي شكل منتج واحد فقط
/// </summary>
public class Product:BaseEntity
{
    /// <summary>
    /// الإسم الأساسي للصنف
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// وصف الصنف
    /// </summary>
    public string Description { get; set; }
    /// <summary>
    /// سعر تكلفة الصنف و يمتل ايضا اخر سعر شراء
    /// </summary>
    public decimal CostPrice { get; set; }
    /// <summary>
    /// سعر بيع الصنف
    /// </summary>
    public decimal SalePrice { get; set; }
    /// <summary>
    /// متوسط سعر تكلفة الشراء
    /// </summary>
    public decimal AveragePrice { get; set; }



    /// <summary>
    /// معرف المستأجر   
    /// </summary>
    public Guid TenantId { get; set; }
    public Tenant? Tenant { get; set; }






    public ICollection<ProductUnit> ProductUnits { get; set; } = new List<ProductUnit>();

    public ICollection<ProductBarCode> ProductBarCodes { get; set; } = new List<ProductBarCode>();

    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();

}
