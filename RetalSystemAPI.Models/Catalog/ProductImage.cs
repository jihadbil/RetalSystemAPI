using System;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Catalog;

/// <summary>
/// يمثل ملفات وصور المنتج في النظام، ويدعم تعيين صورة افتراضية عامة للصنف أو تخصيص صورة لكل عبوة أو باركود على حدة.
/// </summary>
// كلاس كيان صورة المنتج
public class ProductImage : TenantBaseEntity    
{
    /// <summary>
    /// مسار تخزين ملف الصورة على الخادم أو رابط خارجي معتمد للصورة.
    /// </summary>
    // خاصية مسار أو رابط الصورة الإلزامي
    public required string ImageUrl { get; set; }

    /// <summary>
    /// يحدد ما إذا كانت هذه هي الصورة الافتراضية الرئيسية التي تظهر للصنف في الواجهات ونقاط البيع.
    /// </summary>
    // خاصية مؤشر الصورة الافتراضية الرئيسية
    public bool IsDefault { get; set; } = false;

    /// <summary>
    /// معرّف المنتج الأساسي الذي تتبع له هذه الصورة.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المنتج
    public Guid ProductId { get; set; } 

    /// <summary>
    /// كائن المنتج الأساسي المرتبط بالصورة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المنتج
    public Product? Product { get; set; }

    /// <summary>
    /// معرّف الباركود المحدد في حال كانت الصورة مخصصة لعبوة أو متغير معين (اختياري).
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف الباركود
    public Guid? BarcodeId { get; set; }

    /// <summary>
    /// كائن الباركود المرتبط بالصورة المخصصة (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان الباركود
    public ProductBarCode? BarcodeCode { get; set; }
}
