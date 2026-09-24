using System;
using RetalSystemAPI.Models.Catalog;
using RetalSystemAPI.Models.Common;

namespace RetalSystemAPI.Models.Warehouses;

/// <summary>
/// يمثل بنداً واحداً تفصيلياً من بنود أمر وسند التحويل المخزني يحدد الصنف والباركود والكمية المحولة.
/// </summary>
// كلاس كيان بند أمر التحويل المخزني
public class StockTransferItem : TenantBaseEntity
{
    /// <summary>
    /// معرّف سند التحويل المخزني الأساسي الذي يتبع له هذا البند.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف أمر التحويل
    public Guid StockTransferId { get; set; }

    /// <summary>
    /// كائن أمر وسند التحويل المخزني المرتبط (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان أمر التحويل
    public StockTransfer StockTransfer { get; set; } = null!;

    /// <summary>
    /// معرّف الصنف والمنتج المنقول.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف المنتج
    public Guid ProductId { get; set; }

    /// <summary>
    /// كائن المنتج المرتبط بالبند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان المنتج
    public Product Product { get; set; } = null!;

    /// <summary>
    /// معرّف الباركود أو العبوة المحددة عند التحويل من أو إلى مستودع تخزين رئيسي.
    /// </summary>
    // خاصية المفتاح الأجنبي لمعرف باركود الصنف
    public Guid? ProductBarCodeId { get; set; }

    /// <summary>
    /// كائن باركود الصنف المرتبط بالبند (خاصية التنقل).
    /// </summary>
    // خاصية الملاحة لكيان باركود الصنف
    public ProductBarCode? ProductBarCode { get; set; }

    /// <summary>
    /// الكمية الفعلية المحولة من هذا الصنف بين المستودعين.
    /// </summary>
    // خاصية الكمية المحولة
    public int Quantity { get; set; }

    /// <summary>
    /// ملاحظات وإيضاحات خاصة بهذا البند المنقول.
    /// </summary>
    // خاصية ملاحظات البند المنقول
    public string? Notes { get; set; }
}
