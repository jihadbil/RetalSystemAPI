using System;
using System.Collections.Generic;
using System.Text;
using RetalSystemAPI.Models.Common;
namespace RetalSystemAPI.Models.Catalog;
/// <summary>
/// يمتل صور المنتج ايضا تم تصميمه بحيث يدعم صورة لكل نكهة على حدى او بشكل عام
/// </summary>
public class ProductImage:BaseEntity    
{
    /// <summary>
    /// عنوان مسار الصورة على السيرفر او رابط خارجي للصورة  
    /// </summary>
    public required string ImageUrl { get; set; }
    /// <summary>
    /// يمتل ان كانت الصورة الأفتراضية للصنف التي تظهر بشكل عام
    /// </summary>
    public bool IsDefault { get; set; } = false;


    /// <summary>
    /// معرف الصنف الدي تنتمي اليه مجموعة الصور
    /// </summary>
    public Guid ProductId { get; set; } 
    public Product? Product { get; set; }   


    /// <summary>
    /// باركود الصنف الدي تنتمي اليه لاصورة الواحدة
    /// </summary>
    public Guid? BarcodeId { get; set; }

    public ProductBarCode? BarcodeCode { get; set; }
}
