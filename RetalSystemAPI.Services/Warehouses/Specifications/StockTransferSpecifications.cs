using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Services.Warehouses.Specifications;

/// <summary>
/// مواصفات استعلامات أوامر التحويل المخزني مع تضمين المستودع المصدر والمستودع الهدف وبنود التحويل.
/// </summary>
public class StockTransferWithDetailsSpec : BaseSpecification<StockTransfer>
{
    /// <summary>جلب كافة أوامر التحويل مرتبة تنازلياً بتاريخ التحويل</summary>
    public StockTransferWithDetailsSpec()
    {
        AddInclude(s => s.FromWarehouse);
        AddInclude(s => s.ToWarehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(s => s.TransferDate);
    }

    /// <summary>جلب أمر تحويل محدد بالمعرف مع تفاصيله</summary>
    /// <param name="id">معرف أمر التحويل</param>
    public StockTransferWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        AddInclude(s => s.FromWarehouse);
        AddInclude(s => s.ToWarehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>جلب أمر تحويل برقم التحويل</summary>
    /// <param name="transferNumber">رقم أمر التحويل</param>
    public StockTransferWithDetailsSpec(string transferNumber) : base(s => s.TransferNumber == transferNumber)
    {
        AddInclude(s => s.FromWarehouse);
        AddInclude(s => s.ToWarehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    /// <summary>فلترة أوامر التحويل بالمستودع المصدر والمستودع الهدف والحالة والتاريخ والبحث برقم التحويل</summary>
    public StockTransferWithDetailsSpec(
        Guid? fromWarehouseId,
        Guid? toWarehouseId,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null)
        : base(s => (!fromWarehouseId.HasValue || s.FromWarehouseId == fromWarehouseId.Value) &&
                    (!toWarehouseId.HasValue || s.ToWarehouseId == toWarehouseId.Value) &&
                    (!status.HasValue || s.Status == status.Value) &&
                    (!fromDate.HasValue || s.TransferDate >= fromDate.Value) &&
                    (!toDate.HasValue || s.TransferDate <= toDate.Value) &&
                    (string.IsNullOrWhiteSpace(search) || s.TransferNumber.Contains(search)))
    {
        AddInclude(s => s.FromWarehouse);
        AddInclude(s => s.ToWarehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(s => s.TransferDate);
    }
}

/// <summary>
/// مواصفة القوائم والترقيم لأوامر التحويل المخزني — المستودعان وبنود الجذر فقط (لعدد البنود)
/// دون التضمينات العميقة Items.Product / Items.ProductBarCode التي تحتاجها استعلامات التفاصيل فقط.
/// </summary>
public class StockTransferListSpec : BaseSpecification<StockTransfer>
{
    /// <summary>جلب كافة أوامر التحويل للقوائم مرتبة تنازلياً بتاريخ التحويل</summary>
    public StockTransferListSpec()
    {
        AddInclude(s => s.FromWarehouse);
        AddInclude(s => s.ToWarehouse);
        AddInclude(s => s.Items);
        ApplyOrderByDescending(s => s.TransferDate);
    }

    /// <summary>فلترة قوائم أوامر التحويل بالمستودع المصدر والمستودع الهدف والحالة والتاريخ والبحث</summary>
    public StockTransferListSpec(
        Guid? fromWarehouseId,
        Guid? toWarehouseId,
        StockTransferStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? search = null)
        : base(s => (!fromWarehouseId.HasValue || s.FromWarehouseId == fromWarehouseId.Value) &&
                    (!toWarehouseId.HasValue || s.ToWarehouseId == toWarehouseId.Value) &&
                    (!status.HasValue || s.Status == status.Value) &&
                    (!fromDate.HasValue || s.TransferDate >= fromDate.Value) &&
                    (!toDate.HasValue || s.TransferDate <= toDate.Value) &&
                    (string.IsNullOrWhiteSpace(search) || s.TransferNumber.Contains(search)))
    {
        AddInclude(s => s.FromWarehouse);
        AddInclude(s => s.ToWarehouse);
        AddInclude(s => s.Items);
        ApplyOrderByDescending(s => s.TransferDate);
    }
}
