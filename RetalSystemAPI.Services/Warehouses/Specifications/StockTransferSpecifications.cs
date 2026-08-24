using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Enums;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Services.Warehouses.Specifications;

/// <summary>
/// تخصيصات استعلامات أوامر التحويل المخزني مع تفاصيل المستودعات والبنود.
/// </summary>
public class StockTransferWithDetailsSpec : BaseSpecification<StockTransfer>
{
    public StockTransferWithDetailsSpec()
    {
        AddInclude(s => s.FromWarehouse);
        AddInclude(s => s.ToWarehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
        ApplyOrderByDescending(s => s.TransferDate);
    }

    public StockTransferWithDetailsSpec(Guid id) : base(s => s.Id == id)
    {
        AddInclude(s => s.FromWarehouse);
        AddInclude(s => s.ToWarehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

    public StockTransferWithDetailsSpec(string transferNumber) : base(s => s.TransferNumber == transferNumber)
    {
        AddInclude(s => s.FromWarehouse);
        AddInclude(s => s.ToWarehouse);
        AddInclude("Items.Product");
        AddInclude("Items.ProductBarCode");
    }

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
