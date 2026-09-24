using System;
using System.Collections.Generic;
using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Warehouses;
using RetalSystemAPI.Models.Purchase;

namespace RetalSystemAPI.Models.Catalog;

/// <summary>
/// يمثل بيانات الباركود للصنف، لدعم تعدد الأكواد والعبوات والقطع للصنف الواحد (مثل باركود الحبة، الكرتونة، النكهة).
/// </summary>
// كلاس كيان باركود الصنف
public class ProductBarCode : TenantBaseEntity
{
    /// <summary>
    /// رمز الباركود الرقمي أو الأبجدي للصنف (مثال: EAN-13, UPC).
    /// </summary>
    // خاصية نص كود الباركود الإلزامي
    public required string BarCode { get; set; }

    /// <summary>
    /// عنوان أو اسم العبوة/المتغير المرتبط بهذا الباركود (مثال: عصير النسيم - برتقال 250 مل).
    /// </summary>
    // خاصية مسمى أو نكهة العبوة للباركود
    public required string Title { get; set; }

    /// <summary>
    /// وصف وإيضاح تفاصيل هذا الباركود أو العبوة.
    /// </summary>
    // خاصية وصف الباركود الاختياري
    public string? Description { get; set; }

    /// <summary>
    /// معرّف الصنف والمنتج الأساسي الذي ينتمي إليه هذا الباركود.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المنتج
    public Guid ProductId { get; set; }

    /// <summary>
    /// كائن المنتج الأساسي المرتبط بالباركود (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المنتج
    public Product? Product { get; set; }

    /// <summary>
    /// قائمة الصور المرتبطة بعبوة أو شكل هذا الباركود (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة صور الباركود
    public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();    

    /// <summary>
    /// قائمة أرصدة هذا الباركود في مستودعات التخزين الرئيسية (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة أرصدة مستودعات التخزين
    public ICollection<StorgeStock> StorgeStocks { get; set; } = new List<StorgeStock>();

    /// <summary>
    /// قائمة بنود أوامر الشراء الصادرة لهذا الباركود (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لمجموعة بنود أوامر الشراء
    public ICollection<PurchaseOrderItem> PurchaseOrderItems { get; set; } = new List<PurchaseOrderItem>();
}
