using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Warehouses;

/// <summary>
/// يمثل رصيد المخزون في مستودعات التخزين الرئيسية، ويتعامل بدقة على مستوى كل باركود وعبوة ونكهة ولون على حدة.
/// </summary>
// كلاس كيان رصيد مستودع التخزين الرئيسي
public class StorgeStock : TenantBaseEntity
{
    /// <summary>
    /// معرّف مستودع التخزين الذي توجد به البضاعة المخزنة.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المستودع
    public Guid WarehouseId { get; set; }

    /// <summary>
    /// كائن مستودع التخزين المرتبط (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المستودع
    public Warehouse Warehouse { get; set; } = null!;

    /// <summary>
    /// معرّف الباركود المحدد للعبوة أو النكهة أو اللون المخزن.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف باركود الصنف
    public Guid ProductBarcodeId { get; set; }

    /// <summary>
    /// كائن باركود الصنف المرتبط بالرصيد (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان باركود الصنف
    public ProductBarCode ProductBarcode { get; set; } = null!;

    /// <summary>
    /// الكمية الفعلية المتوفرة حالياً في المستودع الرئيسي.
    /// </summary>
    // خاصية الكمية المتوفرة في المخزن
    public int Quantity { get; set; }

    /// <summary>
    /// المستوى الأدنى للمخزون (نقطة إعادة الطلب من المورد).
    /// </summary>
    // خاصية الحد الأدنى لمخزون المستودع
    public int MinStockLevel { get; set; }
}
