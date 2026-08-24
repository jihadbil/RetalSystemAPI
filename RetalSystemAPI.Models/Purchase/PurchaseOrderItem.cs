using RetalSystemAPI.Models.Common;
using RetalSystemAPI.Models.Catalog;

namespace RetalSystemAPI.Models.Purchase;

/// <summary>
/// يمتل محتويات الطلبية 
/// </summary>
public class PurchaseOrderItem : TenantBaseEntity
{
    /// <summary>
    /// يمتل معرف الطلبية الدي ينتمي لها هذا المحتوي
    /// </summary>
    public Guid PurchaseOrderId { get; set; }
    public PurchaseOrder PurchaseOrder { get; set; } = null!;


    /// <summary>
    /// معرف الباركود للنكهة او اللون
    /// </summary>
    public Guid ProductBarCodeId { get; set; }
    public ProductBarCode ProductBarCode { get; set; } = null!;
    /// <summary>
    /// الكمية المطلوبة من هذا الصنف
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// سعر القطعة حسب اخر عملية شراء
    /// </summary>
    public decimal UnitPrice { get; set; }

    /// <summary>
    /// الاجمالي
    /// </summary>
    public decimal LineTotal { get; set; }


 
}
