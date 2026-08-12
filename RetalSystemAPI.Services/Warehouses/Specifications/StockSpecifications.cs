using System;
using RetalSystemAPI.DataAccess.Specifications;
using RetalSystemAPI.Models.Warehouses;

namespace RetalSystemAPI.Services.Warehouses.Specifications;

/// <summary>
/// تخصيصات استعلامات مخزون المخازن (StorgeStock).
/// </summary>
public class StorgeStockWithDetailsSpec : BaseSpecification<StorgeStock>
{
    public StorgeStockWithDetailsSpec()
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
    }

    public StorgeStockWithDetailsSpec(Guid warehouseId)
        : base(s => s.WarehouseId == warehouseId)
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
    }

    public StorgeStockWithDetailsSpec(Guid warehouseId, Guid productBarcodeId)
        : base(s => s.WarehouseId == warehouseId && s.ProductBarcodeId == productBarcodeId)
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
    }
}

/// <summary>
/// تخصيص استعلامات رصيد التخزين الأدنى (Low Storge Stock).
/// </summary>
public class LowStorgeStockSpec : BaseSpecification<StorgeStock>
{
    public LowStorgeStockSpec(Guid? warehouseId = null)
        : base(s => s.Quantity <= s.MinStockLevel && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value))
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.ProductBarcode);
        AddInclude("ProductBarcode.Product");
    }
}

/// <summary>
/// تخصيصات استعلامات مخزون صالات العرض (ShowroomStock).
/// </summary>
public class ShowroomStockWithDetailsSpec : BaseSpecification<ShowroomStock>
{
    public ShowroomStockWithDetailsSpec()
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
    }

    public ShowroomStockWithDetailsSpec(Guid warehouseId)
        : base(s => s.WarehouseId == warehouseId)
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
    }

    public ShowroomStockWithDetailsSpec(Guid warehouseId, Guid productId)
        : base(s => s.WarehouseId == warehouseId && s.ProductId == productId)
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
    }
}

/// <summary>
/// تخصيص استعلامات رصيد صالة العرض الأدنى (Low Showroom Stock).
/// </summary>
public class LowShowroomStockSpec : BaseSpecification<ShowroomStock>
{
    public LowShowroomStockSpec(Guid? warehouseId = null)
        : base(s => s.Quantity <= s.MinStockLevel && (!warehouseId.HasValue || s.WarehouseId == warehouseId.Value))
    {
        AddInclude(s => s.Warehouse);
        AddInclude(s => s.Product);
    }
}
